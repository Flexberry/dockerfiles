#! /usr/bin/env python
import yaml, os, sys, json

inventory = yaml.safe_load(sys.stdin)
print'inventory', json.dumps(inventory, indent=2)

hostList = {}
groupList = {}
for child in inventory['all']['children']:
  if 'children' in inventory['all']['children'][child]:
    groupList[child] = {}
    for group in inventory['all']['children'][child]['children']:
       groupList[child][group] = {}
  else:
    hostList[child] = {}
    for host in inventory['all']['children'][child]['hosts']:
      hostList[child][host] = inventory['all']['children'][child]['hosts'][host]

print 'hostList=', json.dumps(hostList, indent=4)
print 'groupList=', json.dumps(groupList, indent=4)

sys.exit()

hosts = {}
for child in inventory['all']['children']:
  for Host in inventory['all']['children'][child]['hosts']:
    host=Host
    user='icsadmin'
    hostInfo = inventory['all']['children'][child]['hosts'][host]
    if not type(hostInfo) is dict:
      hosts[host] = user
      continue
    if 'ansible_ssh_host' in hostInfo:
      host = hostInfo['ansible_ssh_host']
    if 'ansible_host' in hostInfo:
      host = hostInfo['ansible_host']
    if 'ansible_ssh_user' in hostInfo:
      user = hostInfo['ansible_ssh_user']
    if 'ansible_user' in hostInfo:
      user = hostInfo['ansible_user']
    hosts[host] = user
    #print 'child=', child, 'Host=', Host, 'host=', host,'user=' , user, 'hostInfo=', hostInfo
#print 'HOSTS=', hosts

for host in hosts:
  user = hosts[host]
  cmd = 'ssh-copy-id '+ user + '@' + host
  #print 'CMD=', cmd
  os.system(cmd)
