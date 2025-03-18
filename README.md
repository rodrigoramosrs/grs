# 🌀 Github Release Sync (GRS)

Welcome to **GitHub Release Sync** 🚀—your ultimate partner in keeping your software up-to-date with the latest releases from GitHub. Why sweat over manual updates when GRS has your back? 💡

## ✨ What does GRS do?

GRS automates the process of syncing and downloading the newest versions of your favorite repositories straight from GitHub. Here’s why it’s awesome:  
1️⃣ **Stay Fresh**: Always keep your software updated to the latest release.  
2️⃣ **Hassle-Free**: No more manual digging through GitHub repos.  
3️⃣ **Highly Customizable**: Choose repositories or use raw URLs—the choice is yours!  

## 🛠 How it Works?

With a simple command-line interface powered by Spectre.Console, GRS lets you:
- Specify a raw GitHub repo URL using `--url`.
- Provide the repo `--owner` and `--repo` names instead.
- Stay in control with options like `--current-version` and `--save-dir`.

For example:
```bash
grs update --url https://raw.github.com/user/repo --save-dir ./local_path
grs update --owner user --repo repository --save-dir ./local_path
```bash

## 💪 Why Choose GRS?

- **Effortless Management**: Sync seamlessly and always stay ahead.
- **Flexibility**: Use parameters that work for YOU.  
- **Modern CLI Love**: Enjoy a snazzy CLI interface with Figlet-styled headers and ANSI colorization. 🌈  

## 🚀 Quick Start Guide

1. Clone this repository.  
2. Install dependencies.  
3. Run with your desired parameters:

```bash
grs update --url <RAWREPOURL> --current-version 1.0 --save-dir ./local_path --force
```bash

## 💻 Under the Hood

Built on a strong foundation of:
- [Spectre.Console](https://spectreconsole.net/) 🖥️ - For a modern CLI experience.
- C# Dependency Injection 🧩 - Flexible and extensible.
- ⚙️ Services like `MainAppService` and `GithubReleaseService`.

## 🎉 Enjoy the Ride!

Let **Github Release Sync** take care of your software versioning so you can focus on what truly matters—building awesome things. 🌟

---


💡 *Sync. Save. Stay Updated. That’s GRS!* 🛠️
