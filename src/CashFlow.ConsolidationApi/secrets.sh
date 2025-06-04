#!/bin/bash

# Cria o segredo do MySQL
aws secretsmanager create-secret \
  --name "prod/consolidation/mysql" \
  --description "Credenciais MySQL para Consolidation API" \
  --secret-string '{
    "host": "mysql-service.cashflow.svc.cluster.local",
    "port": "3306",
    "database": "accountDB",
    "username": "user",
    "password": "SenhaSuperSegura123!"
  }' \
  --region us-east-1  # Altere para sua região

# Cria o segredo do Redis
aws secretsmanager create-secret \
  --name "prod/consolidation/redis" \
  --description "Configurações Redis para Consolidation API" \
  --secret-string '{
    "host": "redis-service.cashflow.svc.cluster.local",
    "port": "6379"
  }' \
  --region us-east-1  # Mantenha a mesma região do MySQL

echo "Segredos criados com sucesso!"