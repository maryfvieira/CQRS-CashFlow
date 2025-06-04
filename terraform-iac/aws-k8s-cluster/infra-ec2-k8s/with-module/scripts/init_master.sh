#!/bin/bash
set -ex

kubeadm init --pod-network-cidr=10.244.0.0/16

mkdir -p $HOME/.kube
cp /etc/kubernetes/admin.conf $HOME/.kube/config
chown $(id -u):$(id -g) $HOME/.kube/config

# Aplicar Flannel
kubectl apply -f https://raw.githubusercontent.com/coreos/flannel/master/Documentation/kube-flannel.yml

# Gerar comando join
JOIN_CMD=$(kubeadm token create --print-join-command)

# Armazenar em local acessível
aws ssm put-parameter \
  --name "/k8s/join-command" \
 --value "$JOIN_CMD" \
  --type "String" \
  --overwrite \
  --region sa-east-1
