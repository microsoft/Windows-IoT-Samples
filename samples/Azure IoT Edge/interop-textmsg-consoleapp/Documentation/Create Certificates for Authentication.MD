# Interop Console App with Linux Edge Module
## Progress

- [x] [Introduction](../README.md)  
- [x] [Step 1 - Setup Development Environment](./Setup%20DevVM.MD)   
- [x] [Step 2 - Setup Azure Resources](./Setup%20Azure%20Resources.MD)  
- [x] [Step 3 - Develop and publish the IoT Edge Linux module](./Develop%20and%20publish%20the%20IoT%20edge%20Linux%20module.MD)  
- [ ] **Step 4 - Create Certificates for Authentication**  
- [ ] [Step 5 - Develop the Windows C# Console Application](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
- [ ] [Step 6 - Configuring the IoT Edge Device](./Configuring%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 7 - Deploy the Modules onto the IoT Edge Device](./Deploy%20the%20Modules%20onto%20the%20IoT%20Edge%20Device.MD)  
- [ ] [Step 8 - Run the Console Application](./Run%20the%20Console%20Application.MD)  
---
# Step 4: Create Certificates for Authentication
This part describes the relevant steps to create sample certificates for the authentication between downstream device and IoT edge.

* **To create a sample PKI with certificate validity for one month**  
 Complete the steps within the [Create certificates](https://docs.microsoft.com/azure/iot-edge/tutorial-machine-learning-edge-05-configure-edge-device#create-certificates) section of [Tutorial: Configure an IoT Edge device](https://docs.microsoft.com/azure/iot-edge/tutorial-machine-learning-edge-05-configure-edge-device).  
    > **Note:** You only need to generate certificates and do not need to upload them to Azure Key Vault as described in the section following Create certificates.. 

* **To generate to generate customizable test certificates**  
Follow the steps described in [Generate test certificates for Edge Gateway](https://github.com/Azure-Samples/IoTEdgeAndMlSample/tree/master/CreateCertificates) to create sample certificates for use for the interop solution. It generates certificates with default subject name "turbofanGateway" and 30 days validity. To extend the validity of certificates (or) to change the subject name, make the following modifications to **dockerfile** then re-build the docker image.

    * _To extend the certificate validity to 365 days_
        1. Replace `RUN yum -y install openssl git \` with `RUN yum -y install openssl git sed \`
      
        1. Add following extra line after `&& chmod 700 startup.sh`
            ```powershell
            RUN sed -i -e '/days_till_expire/s/30/365/' certGen.sh
            ```

    * _To customize the subject name_  
        * add the below line before the "ENTRYPOINT [ "/work/startup.sh" ]
            ```powershell
            RUN sed -i -e 's/turbofanGateway/sampleedgedevice/' startup.sh
            ```
You can also use your own root CA certificate in case you have a (test) PKI for downstream devices and edge devices. In any of the two latter cases, replace the private key, certificate chain, and root CA cert.

Go to [Next Step](./Develop%20the%20Windows%20C%23%20Console%20Application.MD)  
