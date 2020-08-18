# Step 6: Configuring the IoT Edge Device.
In this part we configure the Azure IoT Edge for Linux instance on the IoT edge device.

## Steps on the Windows OS on the IoT Edge Device
In the next steps we copy the sample certificates to the Linux environment. Copy the private key, certificate chain, root CA cert for authenticating the IoT Edge Device from the development VM to this Windows IoT device ('new-edge-device.key.pem', 'new-edge-device-full-chain.cert.pem', 'azure-iot-test-only.root.ca.cert.pem') to the '~/certs' folder.
*   WSL2: Copy the files, for instance, via accessing the Linux filesystem through the patch '\wsl$' in File Explorer
*   Linux VM: Use SSH to create a folder in the Linux environment and scp to copy the private key, certificate chain, and root CA cert:
```powershell
ssh <username>@<VM IP/hostname> mkdir -p ~/certs 
scp .\certs\* <username>@<VM IP Address>:~/certs/
```
## Steps in the Linux environment of the IoT Edge Device
*   Log in to the Linux environment
*   WSL2: Follow the [blog post](https://aka.ms/winiot-low) to change into the WSL environment
*   Linux VM: Use SSH to log in to the Linux environment.
```powershell
ssh <username>@<VM IP address>
```
*   Set up the Linux firewall for the interop of Windows application and Linux module. As the Azure Device Client Namespace is used, we use the underlying AMQP protocol for the exchange of messages. We need to specifically allow incoming packets on the ports required by the AMQP protocol. We thus adapt the firewall rules as follows and persist them across reboots:
```bash
sudo iptables -A INPUT -p tcp --dport 443 -j ACCEPT 
sudo iptables -A INPUT -p tcp --dport 5671 -j ACCEPT 
sudo iptables-save | sudo tee /etc/systemd/scripts/ip4save > /dev/null
```
*   Run `sudo chown -R iotedge: ~/certs` to allow Azure IoT Edge to read the certificates.
*   Provision the Azure IoT Edge for Linux configuration:    *   Run `sudo nano /etc/iotedge/config.yaml` to start editing the config file.    
*   To set the connection string for the IoT Edge Device (queried previously, see _Steps in the Azure IoT Portal_), set the device connection string in the configuration file:
```yaml
# Manual provisioning configuration
provisioning:
  source: "manual"
  device_connection_string: "<IoT Edge Device connection string>"
```
*   Enable the following lines to enable using the private key, certificate chain and root CA certificate required for the authenticated communication with the downstream device. This 'configures the edge device to act as a gateway'. The keys were previously deployed onto the VM, see _Steps in the Windows IoT device_.
```yaml
certificates:
  device_ca_cert: "/home/efl-user/certs/new-edge-device-full-chain.cert.pem"
  device_ca_pk: "/home/efl-user/certs/new-edge-device.key.pem"
  trusted_ca_certs: "/home/efl-user/certs/azure-iot-test-only.root.ca.cert.pem"

```
*   If you are on a network with dynamic DNS, then edge modules will automatically be able to resolve the VM’s IP address from its hostname. If you are on a network without dynamic DNS, you will need to assign the VM a static IP address and change the line 'hostname: "…"' to 'hostname: "`<VM IP Address>`'.
*   To save the file and exit nano, press CTRL+x, confirm save and exit with ‘Y’ and press 'Enter'. This concludes the provisioning and configuration.
*   Restart the iotedge by running the below command.
```base
sudo systemctl restart iotedge
```
*   Check the iotedge configured correctly by running the below command.
```bash
sudo iotedge check
```
The command checks if IoT Edge is configured correctly. If you find any errors, please refer to [Troubleshoot your IoT Edge device](https://docs.microsoft.com/azure/iot-edge/troubleshoot).