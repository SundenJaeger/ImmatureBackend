# ImmatureBackend

ImmatureBackend is an ASP.NET Core Web API for grain sample analysis. It takes sample images from a capture app,
returns detected grain boxes, stores confirmed replicates with the technician's measured immature weight, and serves
them to a review dashboard. For each replicate it computes the immature percentage and assigns a grade.

# Features

- **Grain Detection:** Returns grain bounding boxes for an uploaded image. Only boxes with a confidence of 0.5 or
  higher are returned.
- **Replicate Storage:** Saves the image, predicted boxes, confirmed boxes, and weight in PostgreSQL.
- **Automatic Grading:** Percentage is computed from the weight against a 30 g sample, then mapped to a grade.
- **Review Status:** Each replicate is marked `review`, `accepted`, `rejected`, or `retraining`, and the dashboard can
  update it.
- **Image Checks:** Uploads are verified by file signature, so only real JPEG and PNG files are accepted.

> [!NOTE]
> This backend uses [Sentry](https://sentry.io) for error tracking and performance monitoring. Unhandled exceptions, error-level logs, and request traces may be sent to the Sentry project configured through `Sentry__Dsn`. Leave the DSN unset if you do not want any data sent.


# Grading

```
percentage = (weight / 30) * 100
```

| Percentage      | Grade          |
|-----------------|----------------|
| Below 2         | Pr             |
| 2 to 5          | G1             |
| Above 5 to 10   | G2             |
| Above 10 to 15  | G3             |
| Above 15        | Below Standard |

# Usage
> Check the [Swagger Documentation](https://immaturebackend-dev.onrender.com/swagger/index.html)

# Building Locally
> More information can be found in the [Contributing Page](https://github.com/SundenJaeger/ImmatureBackend/blob/master/CONTRIBUTING.md)

# Third-party libraries used in this project

- [**FluentValidation**](https://github.com/FluentValidation/FluentValidation) by Jeremy Skinner under [**Apache-2.0 License**](https://github.com/FluentValidation/FluentValidation/blob/main/License.txt)
- [**FluentResults**](https://github.com/altmann/FluentResults) by Michael Altmann under [**MIT License**](https://github.com/altmann/FluentResults/blob/master/LICENSE)
- [**FileSignatures**](https://github.com/neilharvey/FileSignatures) by Neil Harvey under [**MIT License**](https://github.com/neilharvey/FileSignatures/blob/master/LICENCE)
- [**Serilog**](https://github.com/serilog/serilog) by Serilog Organization under [**Apache-2.0 License**](https://github.com/serilog/serilog/blob/dev/LICENSE)
- [**Sentry .NET**](https://github.com/getsentry/sentry-dotnet) by Functional Software, Inc. under [**MIT License**](https://github.com/getsentry/sentry-dotnet/blob/main/LICENSE)
- [**Swashbuckle.AspNetCore**](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) by Richard Morris under [**MIT License**](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/LICENSE)
- [**Npgsql EF Core Provider**](https://github.com/npgsql/efcore.pg) by Npgsql under [**PostgreSQL License**](https://github.com/npgsql/efcore.pg/blob/main/LICENSE)