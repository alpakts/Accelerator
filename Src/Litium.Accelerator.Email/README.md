# Accelerator Emails Template

Represents the Accelerator Emails Template. It has a Gulp-powered build system. We can write Razor/C# code in these email templates.
After building, it creates email templates in Accelerator's Mail folder `Litium.Accelerator.Mvc\Views\Mail`. Accelerator consumes these email templates, run the inner Razor/C# code to bind data and send emails.

Features:

- Simplified HTML email syntax with [Inky](http://github.com/zurb/inky)
- Sass compilation
- Image compression
- Built-in BrowserSync server
- Full email inlining process

## Installation

To use this template, you can use either NPM or Yarn. We prefer Yarn.
To install NPM: https://www.npmjs.com/get-npm
To install Yarn: https://yarnpkg.com/en/docs/install

Open the current folder in your command line, and install the needed dependencies:

```bash
yarn install
```

## Build

Run `yarn run prod` to inline your CSS into your HTML along with the rest of the build process, copy final html as cshtml into MVC project, which is consumed by Accelerator Mvc project.

```bash
yarn run prod
```

### Speeding Up Your Build

If you create a lot of emails, your build can start to slow down, as each build rebuilds all of the emails in the
repository. A simple way to keep it fast is to archive emails you no longer need by moving the pages into `src/pages/archive`.
You can also move images that are no longer needed into `src/assets/img/archive`. The build will ignore pages and images that
are inside the archive folder.