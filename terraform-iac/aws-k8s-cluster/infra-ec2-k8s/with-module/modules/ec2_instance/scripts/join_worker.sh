#!/bin/bash
exec > >(tee -a /var/log/join_worker.log) 2>&1

set -ex

echo "[INFO] Gerando join command..."
# Buscar comando join do SSM
JOIN_CMD=$(aws ssm get-parameter \
  --name "/k8s/join-command" \
  --with-decryption \
  --region sa-east-1 \
  --query "Parameter.Value" \
  --output text)
echo "[DEBUG] JOIN_COMMAND = $JOIN_COMMAND"

# Executar comando
echo "executar comando join..."
sudo $JOIN_CMD