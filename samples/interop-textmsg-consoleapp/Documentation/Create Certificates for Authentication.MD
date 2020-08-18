# Interop Console App with Linux Edge Module
## Progress

- [x] [Step 1 - Setup Development Environment](./Setup%20Development%20Environment.MD)   
- [x] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [x] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [x] [Step 4 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [ ] [**Step 5 - Create Certificates for Authentication**](./Create%20Certificates%20for%20Authentication.MD)  
- [ ] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---
# Step 5: Create Certificates for Authentication
This part describes the relevant steps to create sample certificates for the authentication between downstream device and IoT edge.
*   The following [link](https://docs.microsoft.com/azure/iot-edge/tutorial-machine-learning-edge-05-configure-edge-device#create-certificates) provides a tutorial on how to create a sample PKI with certificate validity for one month. You only need to generate certificates and do not need to upload them to Azure Key Vault. If you want to generate customizable certificates, please follow the appendix [Customizing Sample Certificates](#_Customizing_Sample_Certificates) .
*   You can also use your own root CA certificate in case you have a (test) PKI for downstream devices and edge devices. In any of the two latter cases, replace the private key, certificate chain, and root CA cert.

Go to [Next Step](./Configuring%20the%20IoT%20Edge%20Device.MD)  