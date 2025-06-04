#!/bin/bash
set -ex

# Aguardar comando do master estar pronto
sleep 60

# Buscar comando do SSM Parameter Store
JOIN_CMD=$(aws ssm get-parameter \
  --name "/k8s/join-command" \
  --with-decryption \
  --region sa-east-1 \
  --query "Parameter.Value" \
  --output text)

# Verificar se o comando foi obtido
if [[ -z "$JOIN_CMD" ]]; then
  echo "Erro: JOIN_CMD vazio. Abortando."
  exit 1
fi

# Executar comando de join
$JOIN_CMD