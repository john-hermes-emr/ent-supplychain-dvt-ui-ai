## Get the Code

Project Template is available at `https://EmersonAutomationSolutions@dev.azure.com/EmersonAutomationSolutions/AS-PMO-Custom-Development-Services/_git/ENT-SupplyChain-DVT-UI`

To create a project for your application follow these steps:
- Run 'git clone https://EmersonAutomationSolutions@dev.azure.com/EmersonAutomationSolutions/AS-PMO-GoldStandard-SIDT/_git/AS-PMO-ReactJS-Template <<project name>>'
Replace Project Name with your application/project name.

- Above steps will clone the ReactJS template for your application. 
- To run the application, install the dependencies using 'npm install'
- Start the application with 'npm start'

Navigate to http://localhost:3000 in your browser.

If you see a home page that prompts you to login, then things are working!  Clicking the **Log in** button will redirect you to the Okta hosted sign-in page.
In order to enable OKTA, Configure your applicaton in OKTA and get the 'Client ID' and 'Issuer' URL. Create a .okta.env file with below 2 variables:
- ISSUER
- CLIENT_ID

You can sign in with username and password from your Okta Directory.
