#!/bin/bash
set -ex

# Instalar dependências
apt-get update
apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release awscli

# Instalar Docker
curl -fsSL https://get.docker.com | sh
systemctl enable docker
systemctl start docker

# Instalar ferramentas do Kubernetes
curl -fsSL https://packages.cloud.google.com/apt/doc/apt-key.gpg | \
  gpg --dearmor -o /etc/apt/trusted.gpg.d/kubernetes.gpg
  
echo "deb https://apt.kubernetes.io/ kubernetes-xenial main" \
  > /etc/apt/sources.list.d/kubernetes.list
  
apt-get update
apt-get install -y kubelet kubeadm kubectl
apt-mark hold kubelet kubeadm kubectl

# Desabilitar swap
swapoff -a
sed -i '/ swap / s/^\(.*\)$/#\1/g' /etc/fstab

# Configurar sysctl
cat <<EOF | tee /etc/sysctl.d/k8s.conf
net.bridge.bridge-nf-call-ip6tables = 1
net.bridge.bridge-nf-call-iptables  = 1
EOF
sysctl --system

# Instalar Helm
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Identificar tipo de node
HOSTNAME=$(hostname)
if [[ "$HOSTNAME" == *"node-0" ]]; then
  bash /scripts/init_master.sh
else
  bash /scripts/join_worker.sh
fi

# === scripts/init_master.sh ===
#!/bin/bash
set -ex

kubeadm init --pod-network-cidr=10.244.0.0/16
mkdir -p $HOME/.kube
cp /etc/kubernetes/admin.conf $HOME/.kube/config
chown $(id -u):$(id -g) $HOME/.kube/config

# Aplicar rede Flannel
kubectl apply -f https://raw.githubusercontent.com/coreos/flannel/master/Documentation/kube-flannel.yml

# Salvar join command no Parameter Store
JOIN_CMD=$(kubeadm token create --print-join-command)
aws ssm put-parameter \
  --name "/k8s/join-command" \
  --value "$JOIN_CMD" \
  --type "String" \
  --overwrite \
  --region sa-east-1

# === scripts/join_worker.sh ===
#!/bin/bash
set -ex
sleep 60
JOIN_CMD=$(aws ssm get-parameter \
  --name "/k8s/join-command" \
  --region sa-east-1 \
  --query "Parameter.Value" \
  --output text)
$JOIN_CMD