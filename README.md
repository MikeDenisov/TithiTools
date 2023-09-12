# TithiCalc

![CI](https://github.com/MikeDenisov/TithiCalc/actions/workflows/dotnet.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/TithiCalc?logo=nuget)](https://www.nuget.org/packages/TithiCalc/) 
[![NuGet](https://img.shields.io/nuget/dt/TithiCalc?logo=nuget)](https://www.nuget.org/packages/TithiCalc/)

TithiCalc is a versatile and precise .NET Core library for calculating Tithis, an essential aspect of Hindu and Vedic lunar calendar systems. Easily determine lunar phases and auspicious times for religious and cultural events. Flexible, efficient, and open source.

## What is a Tithi?

A [**Tithi**](https://en.wikipedia.org/wiki/Tithi) is a fundamental concept in Hindu lunar calendars. It represents a specific lunar day or phase of the moon and is a key element in determining auspicious times for religious and cultural events.

![Moon-Sun Angle](https://upload.wikimedia.org/wikipedia/commons/thumb/d/d2/Tithi_Calculation.jpg/1920px-Tithi_Calculation.jpg)
*The image above illustrates the 12-degree angle of separation between the moon and sun, which is used in Tithi calculations.*

## Installation:

TithiCalc is available as a nuget package from [nuget.org](https://www.nuget.org/packages/TithiCalc)
```
dotnet add package TithiCalc
```

## Examples

You can use the `TithiCalc` library in your C# application to calculate Tithis for a specific date range.

```csharp
// Calculate Tithis within a date range
var tithis = TithiCalc.GetTithiInRange(startDate, endDate);

// Filter Tithis by specific indices
var filteredTithis = TithiCalc.GetTithiInRange(startDate, endDate, indexFilter: new HashSet<int> { 11, 26 });

// Set custom precision
var precisedTithis = TithiCalc.GetTithiInRange(startDate, endDate, precision: 0.0001d);

// Calculate angle between the sun and moon at specific time
var angle = TithiCalc.GetAngle(dateTime);
```

## Contributing:

Contributions to the "TithiCalc" library are welcome! Feel free to submit issues, feature requests, or pull requests to help improve the library.

## License:

This project is licensed under the [GNU License](LICENSE).

## Acknowledgments:

Special thanks to the [CoordinateSharp](https://github.com/Tronald/CoordinateSharp) library for its contribution to celestial calculations in "TithiCalc."
