# Graceterm 
[![Build Status](https://travis-ci.org/matheusneder/graceterm.svg?branch=master)](https://travis-ci.org/matheusneder/graceterm)

Graceterm is a aspnet core middleware that provides implementation to ensure graceful shutdown of the application. 
It was originally written to get zero downtime while performing Kubernetes rolling updates.
The basic concept is: After aplication received a SIGTERM (a signal asking it to terminate), graceterm will hold it alive till all pending requests are completed or a timeout ocurr.

## Usage

Install Nuget package: [Graceterm](https://www.nuget.org/packages/Graceterm/)

Considering a standard AspNet Core application, edit `Configure` method of `Startup` class in order to add the GracetermMiddleware to the pipeline:

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ...)
{
    // If you using a custom logging framework or configuration,
    // you should put the logging's use statament / configuration before Graceterm 
    // in order to Graceterm generate logs according your preferences.
    
    app.UseGraceterm();
    
    // Note that Graceterm need to be prior any other middleware on the pipeline, so it 
    // use statment must be placed before any other middleware but logging.
    
    app.UseMvc();
    ...
}
```

You may also define a custom timeout: `app.UseGraceterm(o => o.TimeoutSeconds = 30);` (default timeout is 60 seconds).

## Contribute

Pull requests are welcome.
