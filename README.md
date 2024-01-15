# Azure Http Function - Zip Blob Files

This Azure Function, named `ZipBlobFilesHttpFunction`, is designed to zip multiple blob files in a specified Azure Storage container. The function is triggered by an HTTP request and expects a JSON payload containing the paths of the files to be zipped.

## Functionality

- **Trigger:** The function is triggered by an HTTP POST request.
- **Authorization:** The function requires Function-level authorization.
- **Validation:** Input validation is performed using the `ModelValidation`, `StringArrayRequiredAttribute` classes.
- **Logging:** The function logs information about the zipping process.
- **Output:** The function returns a JSON object containing the path of the zipped file.

## Configuration

The following settings are expected to be configured:

- `ConnectionString`: The connection string for the Azure Storage account.
- `ContainerName`: The name of the Azure Storage container containing the files to be zipped.

## Usage

To use this function, send an HTTP POST request with the required payload to the function's endpoint. The payload should be a JSON object, containing the `FilePaths` to be zipped.

### Example Request:

```http
POST https://<APP_NAME>.azurewebsites.net/api/ZipBlobFilesHttpFunction?code=<HOST_OR_MASTER_KEY>
Content-Type: application/json
{
  "FilePaths": ["path/to/file1.txt", "path/to/file2.txt"]
}
```
