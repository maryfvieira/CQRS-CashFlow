#!/bin/bash
set -ex

# Cria diretório dos scripts
mkdir -p /scripts

# Copiar os scripts para o path correto
cp /home/ubuntu/bootstrap_k8s.sh /scripts/bootstrap_k8s.sh
cp /home/ubuntu/init_master.sh /scripts/init_master.sh
cp /home/ubuntu/join_worker.sh /scripts/join_worker.sh

# Permitir execução
chmod +x /scripts/*.sh

# Executar bootstrap
/scripts/bootstrap_k8s.sh > /var/log/bootstrap_k8s.log 2>&1
