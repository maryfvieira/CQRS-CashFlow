#!/bin/bash

# 1. Secret para MySQL Password
aws secretsmanager create-secret \
    --name MySql.ConnectionString.Pwd \
    --description "Senha de acesso ao MySQL" \
    --secret-string '{"password":"sua_senha_mysql_aqui"}' \
    --region sa-east-1

# 2. Secret para MongoDB Password
aws secretsmanager create-secret \
    --name MongoDb.ConnectionString.Pwd \
    --description "Senha de acesso ao MongoDB" \
    --secret-string '{"password":"sua_senha_mongodb_aqui"}' \
    --region sa-east-1

# 3. Secret para RabbitMQ Password
aws secretsmanager create-secret \
    --name RabbitMq.Pwd \
    --description "Senha de acesso ao RabbitMQ" \
    --secret-string '{"password":"sua_senha_rabbitmq_aqui"}' \
    --region sa-east-1

# 4. Secret para JWT Key
aws secretsmanager create-secret \
    --name Jwt.Key \
    --description "Chave secreta para geração de tokens JWT" \
    --secret-string '{"key":"sua_chave_jwt_aqui"}' \
    --region sa-east-1