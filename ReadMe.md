# BareE
 
![autobuild](https://github.com/Eightvo/BareE/actions/workflows/autobuild.yml/badge.svg)

BareE is a Code First Game Development Framework. 

# Features

Prebuilt Engine, Developers need only implement game specific logic by creating child classes of the Game and GameSceneBase classes.

Custom Controls defined via .json config files out of the box.

Easily create new Component and Message Types using Component and Message Type attributes.



Supports VR.
***
# How to Build

The repository is a standard Dot Net 5 Solution. The [Tools](https://dotnet.microsoft.com/download) can be obtained from microsoft.

Suggested IDE is [Visual Studio](https://visualstudio.microsoft.com/) Community or Pro. 

Nuget must be configured to find packages from MyGet as described below, or the Veldrid Nuget package references can be replaced with local references to 
Veldrid. 

***
Available on Nuget

[![NuGet](https://img.shields.io/nuget/v/Veldrid.svg)](https://www.nuget.org/packages/BareE) BareE

[![NuGet](https://img.shields.io/nuget/v/Veldrid.svg)](https://www.nuget.org/packages/BareE.EZRend) BareE.EZRend

[![NuGet](https://img.shields.io/nuget/v/Veldrid.svg)](https://www.nuget.org/packages/BareE.Transvoxel) BareE.Transvoxel

***
# MyGet

BareE is built on [Veldrid](https://github.com/mellinoe/veldrid). The Nuget Packages referenced are hosted on [MyGet](https://www.myget.org)

To add this package source in visual studio,

1. Select Tools->Options
2. Select the NuGet Package Managager Node
3. Select the Package Sources SubNod
4. Click the Green "+" button in the top right of the window to add a new package source.
5. Change the name to an Identifier you will recognize (I use Mellinoe MyGet)
6. Set the Source Url to: https://www.myget.org/F/mellinoe/api/v3/index.json
7. Click the "Update" button.

***

This project is Multi-Licensed.
CopyRight Greg Freedline 2021
The PolyForm Noncommercial Licenses 1.0.0 is available. 
Additional Licenses will be available in the future.
https://polyformproject.org/licenses/noncommercial/1.0.0/

Planned Future Licenses:

**Contributor License** - A future free license that allows contributors to use this software for Commercial purposes

**Developer License** - A future paid license covering one year of use in development per developer

**Commercial License** - A future license allowing useage of software in exchange for royalties

Planning a future license type does not guarentee that such a license will be actuated in the future. Planned Licenses are dependent on addtional Research and Legal Advice. There is currently no defined timeline regaurding the implementation and availablitiy of future licenses. Future Licenses are listed for informational purposes only.

