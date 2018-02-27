# Flexberry ORM ODataService Changelog
All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]
### Added

### Fixed

### Changed

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

