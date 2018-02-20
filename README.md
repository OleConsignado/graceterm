# Graceterm 
[![Build Status](https://travis-ci.org/mnconsulting/graceterm.svg?branch=master)](https://travis-ci.org/mnconsulting/graceterm)

Graceterm is a AspNet Core middleware wich provides implementation to ensure graceful shutdown of the application. 
It was originally written to get zero downtime while performing Kubernetes rolling updates.
The basic concept is: After aplication received a SIGTERM (a signal asking it to terminate), graceterm will hold it alive till all pending requests are completed or a timeout ocurr.

## Usage

Install Nuget package: [Graceterm](https://www.nuget.org/packages/Graceterm/)

Considering a standard AspNet Core application, edit `Configure` method of `Startup` by adding `app.UseGraceterm();` statement. The graceterm should be on top of request pipeline to work properly, this means that you must add the `app.UseGraceterm();` statement before any other `app.UseSomething();`.
If you using a custom logging configuration, you should put code wich configures log before the app.UseGraceterm() statament in order to Graceterm generate logs according your preferences.

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ...)
{
    // custom logging configuration here

    app.UseGraceterm();
   
    app.UseMvc();
    ...
}
```

You may also define a custom timeout: `app.UseGraceterm(o => o.TimeoutSeconds = 30);` (default timeout is 60 seconds).

## Contribute

Pull requests are welcome.
