FROM python:3.7-slim

RUN pip install -U pip
RUN pip install numpy==1.17.3 tensorflow==2.0.0 flask pillow

COPY app /app

# By default, we run manual image resizing to maintain parity with CVS webservice prediction results.
# If parity is not required, you can enable faster image resizing by uncommenting the following lines.
# RUN echo "deb http://security.debian.org/debian-security jessie/updates main" >> /etc/apt/sources.list & apt update -y
# RUN apt install -y libglib2.0-bin libsm6 libxext6 libxrender1 libjasper-dev libpng16-16 libopenexr23 libgstreamer1.0-0 libavcodec58 libavformat58 libswscale5 libqtgui4 libqt4-test libqtcore4
# RUN pip install opencv-python

# Expose the port
EXPOSE 80

# Set the working directory
WORKDIR /app

# Run the flask server for the endpoints
CMD python -u app.py