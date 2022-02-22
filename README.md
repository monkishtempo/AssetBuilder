# Asset Builder

## Description

Windows based Asset Builder application. Part of the Toolkit suite.

## Documentation

<https://healthhero.atlassian.net/wiki/spaces/HHE24/pages/3101851684/Asset+Builder>

### CI/CD With Gitlab

Whilst it should be possible to do a .net Framework build using the official Microsoft Windows Framework 4.8 Docker container, it needs to be run from a Windows container (not Linux Container). So far I have not worked out how to (or if you can) get kaniko to use a Windows Container for the build.

I will leave the .gitlab-ci.yml file and the \docker\dockerfile in place in the meantime, but turn off the CI/CD for the project.

#### Enabling/Disabling CI/CD

* From the Project settings, go to 'General'
* expand 'Visibility, project features. permissions'
* In the 'repository' section:
  * Disable 'CI/CD'
  * Click 'Save changes'
