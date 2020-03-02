# Flexberry ORM ODataService Changelog
All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

### Added

1. Handle httpResponseException with OdataError wrapped in targetInvocationException.
2. Support $batch request for transactional update data objects.
3. Support for limits on master details.
4. Support for limits on pseudodetails.

### Changed

1. JavaScriptSerializer replaced with Newtonsoft.Json.JsonConvert for better performance.
2. [BREAKINGCHANGE] Method MapODataServiceDataObjectRoute now requires HttpServer as parameter.
3. At creation of dynamic views of the master in them are added with primary keys.
4. Use common DataObjectCache for all sql queries per http request.
5. [BREAKINGCHANGE] Details BS not apply changes in agregator. Use BS for agregator when details changed.
6. Refactor `DataObjectControllerActivator` to simplify overriding DOC initialization.

### Fixed

1. Fix error with POST request and header "Prefer".
2. Getting objects by primary key with using `$select` and `$expand` query options.
3. Loading masters with common DataObjectCache.
4. Naming of details when exporting data to Excel.
5. Call BS for agregator when details changed in batch requests.

## [5.0.0] - 2018.12.14

### Added

1. Exception handling in user functions.
2. Permissions for masters and details.
3. Export to excel with parameters.
4. The ability to export to an excel function odata.

### Changed

1. Update dependencies.


### Fixed

1. Fix error when query contains same properties.

## [4.1.0] - 2018.02.27
### Added
1. Add support user function geo.intersects.
2. Add support LoadingCustomizationStruct in user functions.
3. Add support actions.
4. Add handler, called after exception appears.
5. In user functions and actions add possibility to return collections of primitive types and enums. In actions add possibility to use primitive types and enums as parameters.
 
### Fixed
1. Fix reading properties of files.
2. Fix error which occured in Mono in method `DefaultODataPathHandler.Parse(IEdmModel model, string serviceRoot, string odataPath)`.
3. Fix errors in work of user functions.
4. Fix error in association object enumeration filtration.
 
### Changed
1. Update dependencies.
2. Update ODataService package version to according ORM package version.

## [2.0.0-beta.5] - 2017-09-02
### Added
* <README.md>
* <CHANGELOG.md>
* <LICENSE.md>
* Publish source code to GitHub public repository.

