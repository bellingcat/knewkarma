# Installation Guide for Knew Karma

Get started with Knew Karma through the simple installation process, whether you prefer using a CLI, Python library,
Docker, or GUI for Windows. Follow these steps to get up and running.

## CLI & Python Library

### Installing from PyPI

Ensure you have Python 3.10 or later on your system to use Knew Karma. You can install the CLI and Python library
directly from PyPI.

```commandline
pip install knewkarma
```

### Installing from the Snap Store

If you prefer installing the snap package instead, you can either run `snap install knewkarma`, or Open the Ubuntu
Software desktop app (assuming snap is already installed), and search for "Knew Karma", and proceed to install the
package.

## Building a Docker Image

If you prefer containerising Knew Karma, you can build the Docker image with the following steps:

1. **Clone the repository** to your system

```
git clone https://github.com/bellingcat/knewkarma.git
``` 

2. **Navigate to the cloned *knewkarma* directory**

```
cd knewkarma
```

3. **Build the Docker image**

```
docker build -t knewkarma .
```

## GUI Installation for Windows

Knew Karma offers a graphical interface for Windows users, providing an intuitive way to access its features. Follow
these steps to install:

## GUI

The GUI instance of Knew Karma is only available for Windows systems, and can be installed by doing the following:

1. **Download the latest setup files from the
   ** [releases page](https://github.com/bellingcat/knewkarma/releases/latest).
2. **Extract the ZIP file** to find the `KnewKarmaSetup.msi` and `setup.exe`.
3. **Install Knew Karma**:
    * If you **don't have the .NET Desktop Runtime** installed or **not sure if it's installed**, run `setup.exe`
    * If you **have the .NET Desktop Runtime**, directly run `KnewKarmaSetup.msi`
4. **Follow the on-screen instructions** to complete the installation, which includes creating desktop and app menu
   shortcuts for easy access.

> This process installs Knew Karma on your Windows system, ready for you to start exploring its capabilities.
