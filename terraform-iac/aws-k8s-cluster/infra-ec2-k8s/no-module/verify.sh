#!/bin/bash

# Verificar instâncias EC2
aws ec2 describe-instances --region sa-east-1

# Verificar VPC
aws ec2 describe-vpcs --region sa-east-1

# Conectar ao nó master (usando o IP do output)
ssh -i ~/.ssh/k8s-cluster-key ubuntu@<IP_PUBLICO>