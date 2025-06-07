resource "aws_instance" "this" {
  count                  = var.instance_count
  ami                    = "ami-0af6e9042ea5a4e3e"
  instance_type          = var.instance_type
  key_name               = var.key_name
  subnet_id              = var.subnet_id
  vpc_security_group_ids = var.security_group_ids
  iam_instance_profile   = var.instance_profile
  associate_public_ip_address = true 

  tags = {
    Name = "k8s-node-${count.index}"
  }

  provisioner "file" {
    source      = "${path.module}/scripts/bootstrap_k8s.sh"
    destination = "/home/ubuntu/scripts/bootstrap_k8s.sh"

    connection {
      type        = "ssh"
      user        = "ubuntu"
      private_key = file(var.private_key_path)
      host        = self.public_dns
      timeout = "10m"
    }
  }

  provisioner "remote-exec" {
    inline = [
      "chmod +x /home/ubuntu/scripts/bootstrap_k8s.sh",
      "sudo /home/ubuntu/scripts/bootstrap_k8s.sh ${count.index} ${var.instance_count} > /var/log/bootstrap.log 2>&1"
    ]

    connection {
      type        = "ssh"
      user        = "ubuntu"
      private_key = file(var.private_key_path)
      host        = self.public_dns
      timeout     = "10m"
    }
  }
}
