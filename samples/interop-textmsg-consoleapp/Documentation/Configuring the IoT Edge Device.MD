# Interop Console App with Linux Edge Module
## Progress

- [x] [Introduction](../README.md)  
- [x] [Step 1 - Setup Development Environment](./Setup%20DevVM.MD)   
- [x] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [x] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [x] [Step 4 - Create Certificates for Authentication](./Create%20Certificates%20for%20Authentication.MD)  
- [x] [Step 5 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [ ] **Step 6 - Configuring the IoT Edge Device**  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---

# Step 6: Configuring the IoT Edge Device.
In this part we configure the Azure IoT Edge for Linux instance on the IoT edge device.

1. **Copy Certificates to Linux VM environment**
    * Use SSH to create a folder in the Linux environment  

        ```powershell
        ssh <username>@<VM IP/hostname> mkdir -p ~/certs
        ``` 


    * Use SCP to copy the certificates created in [Step 4](./Create%20Certificates%20for%20Authentication.MD) to the `~/certs` folder of your Linux environment.  

        ```powershell
        scp .\certs\* <username>@<VM IP Address>:~/certs/
        ```
    
1. **Log in to the Linux environment**
    <table>
    <tr><thead><th>WSL2<img width=1000/></th><th>Linux VM<img width=1000/></th></thead</tr>
    <tr>
    <td>Return to WSL and perform steps 5 and 6 as defined in the <a href="https://aka.ms/winiot-low">Linux modules with Azure IoT Edge on Windows 10 IoT Enterprise</a> blog.
    </td>
    <td>Use SSH to log in to the Linux environment.

    ```powershell
    ssh <username>@<VM IP address>
    ```
    </td>
    </tr>
   </table>
   <hr>

1. **Set up the Linux firewall**   
As the Azure Device Client Namespace is used, we use the underlying AMQP protocol for the exchange of messages. We need to specifically allow incoming packets on the ports required by the AMQP protocol. We thus adapt the firewall rules as follows and persist them across reboots:
    ```bash
    sudo iptables -A INPUT -p tcp --dport 443 -j ACCEPT 
    sudo iptables -A INPUT -p tcp --dport 5671 -j ACCEPT 
    sudo iptables-save | sudo tee /etc/systemd/scripts/ip4save > /dev/null
    ```
1. **Read the certificates**  
    Run the following command to allow Azure IoT Edge to read the certificates.
    ```bash
    sudo chown -R iotedge: ~/certs
    ```
1. **Provision the Azure IoT Edge for Linux configuration**  
    To edit config.yaml run the following command:
    ```bash
    sudo nano /etc/iotedge/config.yaml
    ```    
    * Replace `"<IoT Edge Device connection string>"` in the 'Manual provisioning configuration with the connection string that you obtained in Step 2-6.
    
        ```yaml
        # Manual provisioning configuration
        provisioning:
          source: "manual"
          device_connection_string: "<IoT Edge Device connection string>"
        ```

    * Set the location of the certificates that were copied ot the device earlier.
        ```yaml
        certificates:
          device_ca_cert: "/home/efl-user/certs/new-edge-device-full-chain.cert.pem"
          device_ca_pk: "/home/efl-user/certs/new-edge-device.key.pem"
          trusted_ca_certs: "/home/efl-user/certs/azure-iot-test-only.root.ca.cert.pem"
        ```
    **Note:** Skip the next step if you are on a network with dynamic DNS, then edge modules will automatically be able to resolve the VM’s IP address from its hostname. 

    * If you are on a network without dynamic DNS, you will need to assign the VM a static IP address and replace the line
    `hostname: "…"` with `hostname: "<VM IP Address>"`.

 
    * To save the file and exit nano, press <kbd>CTRL</kbd>+<kbd>x</kbd>, confirm save and exit with <kbd>Y</kbd> and press <kbd>Enter</kbd>. This concludes the provisioning and configuration.
1. **Restart IoT Edge**    
    Restart IoT Edge by running the following command.
    ```base
    sudo systemctl restart iotedge
    ```
1. **Check the IoT Edge configuration**  
    Check that IoT Edge is configured correctly by running the below command.
    ```bash
    sudo iotedge check
    ```
    The command checks if IoT Edge is configured correctly.  If you find any errors, please refer to [Troubleshoot your IoT Edge device](https://docs.microsoft.com/azure/iot-edge/troubleshoot).

Go to [Next Step](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
