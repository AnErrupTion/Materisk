# Materisk

Materisk is a programming language (originally a fork of spaghetto by GoldenretrieverYT) designed for high performance, low overhead programs, with the following principles:
- No hidden allocations (except for one language feature!)
- Concise and consistent syntax
- Extremely fast, ahead-of-time compiled code
- Excellent portability for other systems
- Excellent interoperability with other natively compiled languages such as C, Rust, etc...
- OOP-style features without actually being OOP

Materisk achieves these in part thanks to LLVM, which is a core dependency to the project. Without it, Materisk certainly wouldn't exist in its current form!

Materisk limits itself to the ANSI libc specification, which means it is extremely portable and that your code can be compiled for any system following that specification.

Not only that, but Materisk implements many useful and time-saving features from OOP languages (such as instantiation) without becoming OOP, thanks to some compiler magic. However, it doesn't try to implement syntatic sugar (such as foreach loops), which only improve productivity in the short run.

# Getting started

Clone the repository:

```
git clone https://github.com/AnErrupTion/Materisk
```

Build the project:

```
cd Materisk
dotnet build Materisk.sln
```

Build and run the Hello World example:

```
cd bin
./MateriskCLI ../examples/HelloWorld/main.msk
clang ../examples/HelloWorld/output/corelib.o ../examples/HelloWorld/output/main.o -o main
./main
```