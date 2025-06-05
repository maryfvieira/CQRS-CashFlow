#!/bin/bash
exec > >(tee -a /var/log/init_master.log) 2>&1
set -ex

# Inicializar cluster
echo "[INFO] Inicializar cluster..."
sudo kubeadm init --pod-network-cidr=10.244.0.0/16

# Configurar acesso
echo "[INFO] Configurar acesso..."
echo "[INFO] criando diretorio .kube..."
mkdir -p $HOME/.kube
echo "[INFO] copiando /etc/kubernetes/admin.conf..."
sudo cp /etc/kubernetes/admin.conf $HOME/.kube/config
echo "[INFO] dando permissao..."
sudo chown $(id -u):$(id -g) $HOME/.kube/config

# Instalar rede Flannel
echo "[INFO] Instalar rede Flannel..."
kubectl apply -f https://github.com/flannel-io/flannel/releases/latest/download/kube-flannel.yml

# Esperar apiserver
echo "[INFO] Esperar apiserver..."
sleep 15
kubectl get nodes

# Gerar comando de join
echo "[INFO] Gerando join command..."
JOIN_CMD=$(kubeadm token create --print-join-command)
echo "[DEBUG] JOIN_COMMAND = $JOIN_COMMAND"

echo "[INFO] Salvando join command no Parameter Store..."
PUT_RESULT=$(aws ssm put-parameter \
  --name "/k8s/join-command" \
  --type "String" \
  --value "$JOIN_COMMAND" \
  --overwrite \
  --region sa-east-1 2>&1)
  
  EXIT_CODE=$?
  if [ $EXIT_CODE -eq 0 ]; then
    echo "[INFO] Comando salvo com sucesso no Parameter Store"
  else
    echo "[ERROR] Falha ao salvar o comando no Parameter Store"
    echo "[DEBUG] AWS CLI output:"
    echo "$PUT_RESULT"
  fi
