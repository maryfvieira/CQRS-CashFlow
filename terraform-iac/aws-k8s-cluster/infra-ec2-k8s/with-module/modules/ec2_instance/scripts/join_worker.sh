#!/bin/bash
set -ex

# Buscar comando join do SSM
JOIN_CMD=$(aws ssm get-parameter \
  --name "/k8s/join-command" \
  --with-decryption \
  --region sa-east-1 \
  --query "Parameter.Value" \
  --output text)

# Executar comando
sudo $JOIN_CMD