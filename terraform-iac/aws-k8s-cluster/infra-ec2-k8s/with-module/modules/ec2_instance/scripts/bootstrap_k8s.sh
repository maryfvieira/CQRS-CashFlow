#!/bin/bash
set -ex

NODE_INDEX=$1
TOTAL_NODES=$2

# Instalar dependências
sudo apt-get update
sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release awscli

# Instalar Docker
curl -fsSL https://get.docker.com | sudo sh
sudo systemctl enable docker
sudo systemctl start docker

# Instalar Kubernetes
sudo curl -fsSLo /usr/share/keyrings/kubernetes-archive-keyring.gpg https://packages.cloud.google.com/apt/doc/apt-key.gpg
echo "deb [signed-by=/usr/share/keyrings/kubernetes-archive-keyring.gpg] https://apt.kubernetes.io/ kubernetes-xenial main" | sudo tee /etc/apt/sources.list.d/kubernetes.list
sudo apt-get update
sudo apt-get install -y kubelet kubeadm kubectl
sudo apt-mark hold kubelet kubeadm kubectl

# Configurar hostname
sudo hostnamectl set-hostname k8s-node-$NODE_INDEX

# Desabilitar swap
sudo swapoff -a
sudo sed -i '/ swap / s/^\(.*\)$/#\1/g' /etc/fstab

# Configurar sysctl
cat <<EOF | sudo tee /etc/sysctl.d/k8s.conf
net.bridge.bridge-nf-call-ip6tables = 1
net.bridge.bridge-nf-call-iptables  = 1
EOF
sudo sysctl --system

# Instalar Helm
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | sudo bash

# Executar scripts específicos para master/worker
if [ "$NODE_INDEX" -eq "0" ]; then
  sudo /scripts/init_master.sh
else
  # Aguardar master estar pronto
  sleep 120
  sudo /scripts/join_worker.sh
fi