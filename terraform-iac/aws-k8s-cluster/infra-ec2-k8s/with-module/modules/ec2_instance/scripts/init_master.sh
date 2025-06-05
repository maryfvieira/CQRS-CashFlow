#!/bin/bash
set -ex

# Inicializar cluster
sudo kubeadm init --pod-network-cidr=10.244.0.0/16

# Configurar acesso
mkdir -p $HOME/.kube
sudo cp /etc/kubernetes/admin.conf $HOME/.kube/config
sudo chown $(id -u):$(id -g) $HOME/.kube/config

# Instalar rede Flannel
kubectl apply -f https://github.com/flannel-io/flannel/releases/latest/download/kube-flannel.yml

# Espera apiserver
sleep 15
kubectl get nodes

# Gerar comando join
JOIN_CMD=$(kubeadm token create --print-join-command)

# Limpar parametro antigo
aws ssm delete-parameter --name "/k8s/join-command" --region sa-east-1 || true

# Armazenar comando no SSM
aws ssm put-parameter \
  --name "/k8s/join-command" \
  --value "$JOIN_CMD" \
  --type "String" \
  --overwrite \
  --region sa-east-1
