# Graceterm 
[![Build Status](https://travis-ci.org/mnconsulting/graceterm.svg?branch=master)](https://travis-ci.org/mnconsulting/graceterm)

Graceterm is a AspNet Core middleware wich provides implementation to ensure graceful shutdown of the application. 
It was originally written to get zero downtime while performing Kubernetes rolling updates.
The basic concept is: After aplication received a SIGTERM (a signal asking it to terminate), graceterm will hold it alive till all pending requests are completed or a timeout ocurr.

If you're interested in configuring aspnet core app on a Kubernetes cluster to get zero downtime updates and wondering about what needs to be done, you should take look at this aricle: https://github.com/mnconsulting/graceterm/wiki/Zero-downtime-AspNet-Core-and-Kubernetes-rolling-updates.

## Usage

Install Nuget package: [Graceterm](https://www.nuget.org/packages/Graceterm/)

Considering a standard AspNet Core application, edit `Configure` method of `Startup` by adding `app.UseGraceterm()` invocation. The *graceterm* should be on top of request pipeline to work properly, this means that you must add the `app.UseGraceterm()` invocation before any other `app.UseSomething()`.
If you are using a custom logging configuration, you should put the code wich configures log before the `app.UseGraceterm()` in order to Graceterm generate logs according to your preferences.

```cs
using Graceterm;

public class Startup
{
    ...
    
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ...)
    {
        //
        // Custom logging configuration goes here.
        //

        // Add graceterm just after logging configuration and before 
        // any other middleware.
        app.UseGraceterm(); 

        app.UseOtherMiddleware();       
        ...
    }
    ...
}
```
### Options

You may customize Graceterm behavior providing a delegate to `app.UseGraceterm(delegate)` invocation. The available options are describled bellow:

```cs
app.UseGraceterm(options => 
{
    // Timeout
    // How much time Graceterm will hold application alive (default is 60 seconds)
    options.TimeoutSeconds = 15; 
    
    // IgnorePaths
    // Requests to paths starting with the segments ("/path1", "/path2" and "/path3") 
    // will not be handled by Graceterm.
    options.IgnorePaths("/path1", "/path2", "/path3");
    
    // UseCustomPosSigtermIncommingRequestsHandler
    // By default graceterm send a 503 response, with a "503 - Service unavailable" text body 
    // for requests initiated after application has asked to terminated. You may modify this
    // behavior by providing a Func<HttpContext, Task> to handle this requests.    
    options.UseCustomPostSigtermIncommingRequestsHandler(async (httpContext) =>
    {
        httpContext.Response.StatusCode = 503;
        await httpContext.Response.WriteAsync("My custom message!");
    });
});
```

## Contribute

Issues and pull requests are welcome.
