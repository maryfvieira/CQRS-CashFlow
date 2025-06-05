resource "aws_instance" "this" {
  count                  = var.instance_count
  ami                    = "ami-0af6e9042ea5a4e3e"
  instance_type          = var.instance_type
  key_name               = var.key_name
  subnet_id              = var.subnet_id
  vpc_security_group_ids = var.security_group_ids
  iam_instance_profile   = var.instance_profile

  tags = {
    Name = "k8s-node-${count.index}"
  }

  provisioner "file" {
    source      = "${path.module}/scripts/"
    destination = "/tmp/"

    connection {
      type        = "ssh"
      user        = "ubuntu"
      private_key = file(var.private_key_path)
      host        = self.public_ip
    }
  }

  provisioner "remote-exec" {
    inline = [
      "sudo rm -rf /scripts",
      "sudo mv /tmp/scripts /scripts",
      "sudo chmod -R +x /scripts",
      "sudo /scripts/bootstrap_k8s.sh ${count.index} ${var.instance_count} > /var/log/bootstrap.log 2>&1"
    ]

    connection {
      type        = "ssh"
      user        = "ubuntu"
      private_key = file(var.private_key_path)
      host        = self.public_ip
    }
  }
}
