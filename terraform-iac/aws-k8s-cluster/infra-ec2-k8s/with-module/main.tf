module "network" {
  source      = "./modules/network"
  vpc_cidr    = var.vpc_cidr
  subnet_cidr = var.subnet_cidr
  region      = var.region
}

module "security_group" {
  source = "./modules/security_group"
  vpc_id = module.network.vpc_id
  vpc_cidr = var.vpc_cidr
}

module "iam" {
  source = "./modules/iam"
}

module "ec2_instance" {
  source                  = "./modules/ec2_instance"
  instance_count          = var.instance_count
  instance_type           = var.instance_type
  key_name                = var.key_name
  subnet_id               = module.network.subnet_id
  security_group_ids      = [module.security_group.sg_id]
  instance_profile        = module.iam.instance_profile
  private_key_path        = var.private_key_path
  depends_on = [module.iam]
}