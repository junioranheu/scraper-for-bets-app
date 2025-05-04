# ⚽ SCRAPER FOR BETS

A .NET 9 console app built to scrape **real-time football data** from [SofaScore](https://www.sofascore.com/) to assist in smarter, data-driven betting decisions.  

**Status:** ABSOLUTE BETA!

<b>Disclaimer: This project is for educational and informational purposes only. It is not affiliated with SofaScore or any betting platform.</b>

---

## 🧠 About this project
SCRAPER FOR BETS is a .NET 9 console application designed to retrieve real-time football data from SofaScore using browser automation (OpenQA/Selenium).
The goal is to support data-informed betting without relying on paid APIs or shady tipster groups. Built with a clean code mindset for maintainability and scalability.

---

## 📦 Features

- Real-time football data extraction using Selenium with OpenQA  
- Headless (or not, you decide it) scraping (ChromeDriver)  
- Clean Code structure (SOLID principles, separation of concerns)  
- Minimal dependencies for portability
- Match analysis and predictions (coming soon)  

---

## 🔧 Tech stack

- **.NET 9** – Modern, high-performance base  
- **OpenQA.Selenium** – For browser automation and data scraping  
- **Clean Code** – SOLID principles, domain separation  
- **SofaScore** – Real-time match data source (scraped, not official)

---

## 📁 Project structure
/ ScraperForBet.Core<br/>
├── /Enums<br/> 
├── /Helpers<br/> 
│ ├── MiscHelper.cs<br/>
│ ├── ScraperHelper.cs<br/>
├── /Models // Domain entities (Game, Team, etc.)<br/>
├── /Services // Core scraping logic<br/>
│ ├── Scraper.cs<br/>
├── Program.cs // App entry point<br/>
