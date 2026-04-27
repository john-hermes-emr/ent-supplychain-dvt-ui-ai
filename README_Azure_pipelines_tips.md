Tips for using Azure Pipeline:

1. Azure Pipeline file consists of steps for Continuous Integration (CI) pipeline, which includes:
    - Unit Testing
    - SonarCloud Integration
    - Fortify Integration
    - Copying Kubernetes Deployment file (this step is required only if you are deploying the apps to AKS Cluster)
    - Dockerizing the Build
    
2. As part of CI pipeline, team needs to update the variable values in Azure DevOps CI pipeline:
    - SONAR_ServiceConnection (variable for Sonar Service Connection Name)
    - organization_name (variable for Organization Name in Sonar Cloud)
    - SonarProjectName (variable for Project Name from Sonar Cloud)
    - SonarProjectKey (variable for Project Key from Sonar Cloud)
    - FORTIFY_SERVER_URL (variable for Sever URL from Fortify application)
    - FORTIFY_AUTH_TOKEN (variable for Authentication token for Fortify)
    - APP_NAME (variable for Application Name from Fortify)
    - APP_VERSION (variable for Application version from Fortify)
    - dockerId (variable for Azure Container Registry ID)
    - dockerPassword (variable for Azure Container Registry Password)
        
3. Unit Testing   
    Jest is the tool used for automating the unit test during CI pipeline and publish the code coverage report.  

4. SonarCloud Configuration
    SonarCloud is a cloud-based code quality and security service. Continuous code quality and code security is enhanced with SonarCloud Integration.
    Project team needs to request for creating the project in SonarCloud and get below information:
    - SonarCloud URL
    - Organization Name
    - Project Name and Key
    
    Create authentication token to connect to SonarCloud and create a Service Connection in project settings. Use the Service connection name, 
    Sonar Organization name, Project name and key in variables. SonarCloud integration in Azure DevOps follows 3 steps:
    - SonarCloud connection
    - Analyze the code
    - Publish the Quality Gate report

5. Fortify Configuration
    Fortify is an industry leading SAST (Static Application Security Testing) tool. Fortify tool is integrated with Azure DevOps to enable continuous static code analyzer.
    Project team needs to contact Fortify team to register the application and get the Authentication key, Application name and version. Fortify report
    can be generated and pushed to build pipeline.
    SonarCloud integration in Azure DevOps follows below steps:
    - Scan the application
    - Copy Fortify report to Build Artifacts
    - Publish the report

6. Kubernetes resource configuration file
    If you want to host the application on Kubernetes cluster, required Kubernetes resources need to be created. The template includes below resources:
    - Namespace
    - Secret Provider class to get the Digital certificate
    - Deployment
    - Service
    - Ingress
    
    Project team needs to update the variables with their preferred names.     
    The Kubernetes deployment file is copied to build artifact and used in deployment step using kubectl apply command 
        
7. Terraform  
   Terraform is an open-source, infrastructure as code (IaC) that enables to create, change and improve infrastructure. In this template, we have used 
   IaC code to create below resources on Azure cloud:
   - App Service Plan
   - Azure Web App
   
   main.tf and variables.tf files contain the scripts and variable to create the Azure resources. These files are copied to Build Artifact and used in the deployment script.

8. Docker Build configuration
    As part of Continuous Integration Application code will be built with Docker Image and pushed to Azure Container Registry (ACR). Docker file is available at docker/prod folder.
    Docker build command runs the steps from Dockerfile and create the image. The image will be pushed to ACR using docker push command.
    In order to push the image to ACR, login is required. Docker login command is used to authenticate to ACR. ACR User ID and Password are maintained in variable. 


