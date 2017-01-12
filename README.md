# CSharp RiveScript Interpreter

[![Build status](https://ci.appveyor.com/api/projects/status/crwboe7fa6aseqvc/branch/master?svg=true)](https://ci.appveyor.com/project/fabioravila/rivescript-csharp/branch/master)
[![MIT License](https://img.shields.io/github/license/fabioravila/rivescript-csharp.svg)](./LICENSE)

## SYNOPSIS

This is a C# implementation of a RiveScript interpreter, per the Working Draft
at http://www.rivescript.com/wd/RiveScript.html

This code was initially developed based on the official version of the Java interpreter
rivescript-java - https://github.com/aichaos/rivescript-java

## MOTIVATION

As a C# developer sought a different mechanism of AIML + Program# for chatterbot due to
discontinuity or implementation of complication.
In seeking an alternative i came across the RiveScript that with its simplicity and syntax
made me choose this as my new chatterbot script.
As the C# interpreter announced the offical site is not functional, i have decided to make
the C# interpreter, based on the official Java code.

PS: This is my first public library, then I'm sorry for the lack of knowledge of some community standards.

## TASK LIST

- [x] Make a port of actual rivescript-java - Make a port of current Java code, 'shame on me =]'
- [x] Create some basic tests - based on RiveScript 'official' demos and myself
- [x] Push to github on public repository
- [x] Develop a native objects for csharp
- [x] Migrate last fixes and changes from rivescript-java since jan-2016 (some push a lot)
- [ ] Create a RSBot Demo Script
- [ ] Complete test based on rsts
- [ ] Complete the code documentation and adjust appveyor to build and release on tag
- [ ] [...]


## CODE STANDARDS
1. I put the methods with same name as Java lib (pascalCasing) to all imterpreters have same mehtod name
2. I like to use (false == condition) instead (!condition), becous i always miss the "!" out my vision
3. Do not use Linq, to make this code compatible with olders frameworks versions
4. [...]


## DOCUMENTATION

:TODO - C# API documentation

Also check out the [**RiveScript Community Wiki**](https://github.com/aichaos/rivescript/wiki) 
for common design patterns and tips & tricks for RiveScript.

## LICENSE

```
The MIT License (MIT)

Copyright (c) 2015 Fábio Ávila based on Noah Petherbridge 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## AUTHOR

Fábio Ávila, https://br.linkedin.com/in/fabioravila 
based on Noah Petherbridge´s code