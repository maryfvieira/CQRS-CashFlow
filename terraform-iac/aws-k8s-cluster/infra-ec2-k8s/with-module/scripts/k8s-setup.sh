#!/bin/bash
set -ex

# 1. Install dependencies
apt-get update
apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release

# 2. Install Docker
curl -fsSL https://get.docker.com | sh
systemctl enable docker
systemctl start docker

# 3. Install Kubernetes tools
curl -fsSL https://packages.cloud.google.com/apt/doc/apt-key.gpg | \
  gpg --dearmor -o /etc/apt/trusted.gpg.d/kubernetes.gpg

echo "deb https://apt.kubernetes.io/ kubernetes-xenial main" \
  > /etc/apt/sources.list.d/kubernetes.list

apt-get update
apt-get install -y kubelet kubeadm kubectl
apt-mark hold kubelet kubeadm kubectl

# 4. Disable swap
swapoff -a
sed -i '/ swap / s/^\(.*\)$/#\1/g' /etc/fstab

# 5. Sysctl configs
cat <<EOF | tee /etc/sysctl.d/k8s.conf
net.bridge.bridge-nf-call-ip6tables = 1
net.bridge.bridge-nf-call-iptables  = 1
EOF
sysctl --system

# 6. Install Helm
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# 7. Master node init (flag-based)
if [ -f /etc/k8s-master ]; then
  kubeadm init --pod-network-cidr=10.244.0.0/16

  mkdir -p $HOME/.kube
  cp /etc/kubernetes/admin.conf $HOME/.kube/config
  chown $(id -u):$(id -g) $HOME/.kube/config

  # Rede Flannel
  kubectl apply -f https://raw.githubusercontent.com/coreos/flannel/master/Documentation/kube-flannel.yml

  # Salvar comando join
  kubeadm token create --print-join-command > /join_worker.sh
  chmod +x /join_worker.sh
fi

# 8. Worker nodes join (caso já tenha o script)
if [ -f /join_worker.sh ]; then
  bash /join_worker.sh
fi
