resource "aws_instance" "this" {
  count                  = var.instance_count
  ami                    = "ami-0af6e9042ea5a4e3e"
  instance_type          = var.instance_type
  key_name               = var.key_name
  subnet_id              = var.subnet_id
  vpc_security_group_ids = var.security_group_ids
  iam_instance_profile   = var.instance_profile

  user_data = file("${path.module}/scripts/user_data.sh")
  
  tags = {
    Name = "k8s-node-${count.index}"
  }

  # provisioner "file" {
  #   source      = "${path.module}/../../scripts/"
  #   destination = "/root/"
  #   connection {
  #     type = "ssh"
  #     user = "ubuntu"
  #     private_key = file("~/.ssh/your-key.pem")
  #     host = self.public_ip
  #   }
  # }
}