variable "instance_count" {}
variable "instance_type" {}
variable "key_name" {}
variable "subnet_id" {}
variable "security_group_ids" {
  type = list(string)
}
variable "instance_profile" {
  description = "Nome do IAM Instance Profile"
  type        = string
}
variable "private_key_path" {
  description = "Caminho local para a chave privada usada para conexão SSH com a instância EC2"
  type        = string
}
