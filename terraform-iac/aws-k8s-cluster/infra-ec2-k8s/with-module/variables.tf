variable "region" {
  description = "Região AWS"
  default     = "sa-east-1"
}

variable "vpc_cidr" {
  description = "CIDR da VPC"
  default     = "10.0.0.0/16"
}

variable "subnet_cidr" {
  description = "CIDR da Subnet"
  default     = "10.0.1.0/24"
}

variable "instance_count" {
  description = "Número de instâncias EC2"
  default     = 3
}

variable "instance_type" {
  description = "Tipo de instância EC2"
  default     = "t2.micro"
}

variable "key_name" {
  description = "Nome da chave SSH"
  default     = "k8s-cluster-key"
}
variable "private_key_path" {
  description = "Caminho para a chave privada SSH"
}