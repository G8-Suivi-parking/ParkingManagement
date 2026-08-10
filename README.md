# Parking Management

## 1. Présentation du projet

Parking Management est une plateforme web destinée à la gestion
des abonnements de parkings pour les entreprises.

Le backend fournit une API REST permettant de gérer les entreprises,
leurs abonnements et l'historique des opérations réalisées sur les abonnements.

---

## 2. Objectif du backend

Le backend a pour objectif de :

- gérer les entreprises ;
- créer des abonnements ;
- empêcher une entreprise d'avoir plusieurs abonnements actifs ;
- renouveler les abonnements ;
- suspendre les abonnements ;
- terminer les abonnements ;
- enregistrer les raisons de suspension ou de résiliation ;
- conserver l'historique des opérations.

---

## 3. Technologies utilisées

### Backend

- C#
- .NET 10
- ASP.NET Core
- Entity Framework Core

### Base de données

- PostgreSQL

### Documentation et tests API

- Swagger / OpenAPI

### Outils

- Visual Studio Code
- Git
- GitHub

---

## 4. Architecture du projet

Le backend est organisé de la manière suivante :

```text
ParkingManagement.API
│
├── Controllers
│   ├── EntreprisesController.cs
│   └── AbonnementsController.cs
│
├── Data
│   └── ApplicationDbContext.cs
│
├── Models
│   ├── Entreprise.cs
│   ├── Abonnement.cs
│   └── AbonnementHistorique.cs
│
├── Migrations
│
├── Properties
│
├── Program.cs
├── appsettings.json
└── ParkingManagement.API.csproj
```
"# ParkingManagement.API" 
