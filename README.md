# EmailRewriter

## Overview

Sample web app showing how to use Semantic Kernel and Ollama OpenAI integration to build an email rewriter.
 The application uses various plugins and services to enhance the readability and conciseness of emails.

## Features

- Rewrite email content to be concise and to the point.
- Use Semantic Kernel's chat completion service for content rewriting.
- Integrate with Ollama OpenAI API endpoint.
- Utilize OpenTelemetry for logging, tracing, and metrics.

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022
- Ollama OpenAI API endpoint

## Setup

### Clone the Repository


### Configure the Ollama API

Ensure you have access to the Ollama OpenAI API endpoint(You can download Ollama here: https://ollama.com/download). 
Update the endpoint URL in the `Program.cs` files of the projects to point to your Ollama API endpoint.

### Build the Projects

Open the solution in Visual Studio 2022 and build the projects to restore the necessary packages and dependencies.

### Update Configuration

Ensure the `appsettings.json` or any other configuration files are updated with the necessary settings for your environment.


## Running the Application

1. Navigate to the `EmailRewriter.AppHost` project.
2. Run the project using Visual Studio or the .NET CLI


## Using the Application

### Aspire App Host

- Access the web application at `https://localhost:17089/` (or the configured URL).
- Click on the EmailRewriter-Web endpoint to browse to the application.
- Use the provided UI to input email content and get the rewritten version.

## Contributing

Contributions are welcome! Please fork the repository and submit pull requests for any enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.