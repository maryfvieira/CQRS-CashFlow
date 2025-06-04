# 1. VPC
resource "aws_vpc" "k8s_vpc" {
  cidr_block           = var.vpc_cidr
  enable_dns_support   = true
  enable_dns_hostnames = true
  tags = {
    Name = "k8s-vpc"
  }
}

# 2. Subnet
resource "aws_subnet" "k8s_subnet" {
  vpc_id            = aws_vpc.k8s_vpc.id
  cidr_block        = var.subnet_cidr
  availability_zone = "${var.region}a"
  tags = {
    Name = "k8s-subnet"
  }
}

# 3. Security Group
resource "aws_security_group" "k8s_sg" {
  name        = "k8s-security-group"
  description = "Security group para cluster Kubernetes"
  vpc_id      = aws_vpc.k8s_vpc.id

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "Kubelet API"
    from_port   = 10250
    to_port     = 10250
    protocol    = "tcp"
    cidr_blocks = [aws_vpc.k8s_vpc.cidr_block]
  }

  ingress {
    description = "NodePort Services"
    from_port   = 30000
    to_port     = 32767
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# 4. EC2 Instances
resource "aws_instance" "k8s_node" {
  count                  = var.instance_count
  ami                    = "ami-0af6e9042ea5a4e3e" # Ubuntu 22.04 LTS
  instance_type          = var.instance_type
  key_name               = var.key_name
  subnet_id              = aws_subnet.k8s_subnet.id
  vpc_security_group_ids = [aws_security_group.k8s_sg.id]

  tags = {
    Name = "k8s-node-${count.index}"
  }
}