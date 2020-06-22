# Custom Vision Dockerfile
Exported from customvision.ai.

## Build

```bash
docker build -t <your image name> .
```

### Build ARM container on x64 machine

Export "ARM" Dockerfile from customvision.ai. Then build it using docker buildx command.
```bash
docker buildx build --platform linux/arm/v7 -t <your image name> --load .
```

## Run the container locally
```bash
docker run -p 127.0.0.1:80:80 -d <your image name>
```

## Image resizing
By default, we run manual image resizing to maintain parity with CVS webservice prediction results.
If parity is not required, you can enable faster image resizing by uncommenting the lines installing OpenCV in the Dockerfile.

Then use your favorite tool to connect to the end points.

POST http://127.0.0.1/image with multipart/form-data using the imageData key
e.g
    curl -X POST http://127.0.0.1/image -F imageData=@some_file_name.jpg

POST http://127.0.0.1/image with application/octet-stream
e.g.
    curl -X POST http://127.0.0.1/image -H "Content-Type: application/octet-stream" --data-binary @some_file_name.jpg

POST http://127.0.0.1/url with a json body of { "url": "<test url here>" }
e.g.
    curl -X POST http://127.0.0.1/url -d '{ "url": "<test url here>" }'

For information on how to use these files to create and deploy through AzureML check out the readme.txt in the azureml directory.
