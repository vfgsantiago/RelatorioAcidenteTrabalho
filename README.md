# 🛡️ Sistema de Gestão e Reporte de Acidentes de Trabalho

![Badge Status](https://img.shields.io/badge/Status-Concluido-green)
![Badge Type](https://img.shields.io/badge/Focus-HSE%20%2F%20Seguran%C3%A7a-red)
![Badge Access](https://img.shields.io/badge/Access-Public%20%26%20Admin-blue)

> **Segurança em primeiro lugar. Registre, analise e previna.**

Uma plataforma completa para a gestão de segurança do trabalho, composta por um portal público para registro ágil de incidentes e um back-office administrativo para parametrização dinâmica e análise de dados.

---

## 🚧 O Desafio
A burocracia no registro de acidentes muitas vezes leva à subnotificação. Além disso, formulários estáticos de papel não geram dados estruturados, dificultando a análise de causas raízes e a criação de planos de prevenção eficazes.

## ✅ A Solução
Um sistema digital onde qualquer colaborador pode reportar um incidente em segundos. No lado da gestão, a equipe de segurança (SESMT/CIPA) tem total liberdade para criar os questionários e visualizar indicadores em tempo real.

---

## 🏗️ Módulos do Sistema

### 1. 🌐 Portal Público (Reporte)
A porta de entrada para os registros. Projetado para ser simples, rápido e acessível via mobile ou desktop.
* **Acesso Simplificado:** Não requer login complexo para quem está reportando.
* **Registro de Ocorrência:** Interface guiada para descrever o acidente (Onde, Como, Quando).
* **Anonimato Opcional:** Permite que o colaborador escolha se identificar ou não.

### 2. ⚙️ Painel Administrativo (Parametrização)
Onde a inteligência do sistema é configurada.
* **Gerador de Formulários Dinâmicos:** O administrador define **quais perguntas** aparecem no portal público (ex: Múltipla escolha, Texto livre, Data, Upload de foto).
* **Gestão de Categorias:** Criação e edição de tipos de acidentes (ex: "Trajeto", "Típico", "Ergonômico") e níveis de severidade.
* **Sem dependência de TI:** Altere o formulário a qualquer momento sem precisar mexer no código fonte.

### 3. 📊 Dashboard & Analytics
Transformando registros em prevenção.
* **Mapa de Calor:** Identifique quais setores ou horários têm mais incidentes.
* **Indicadores de Gravidade:** Gráficos que mostram a evolução dos acidentes (Com afastamento vs. Sem afastamento).
* **Exportação:** Geração de relatórios para auditorias e reuniões da CIPA.

---


## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# (.NET)
* **Backend/Frontend:** ASP.NET Core (MVC & Web API)
* **Banco de Dados:** Oracle PLSQL
* **Estilização:** Bootstrap / CSS3 / AJAX / JQUERY

  ---

## 🛠️ Metodoloias Utilizadas

* **Arquitetura:** Camadas
* **Padrão:** Repository Pattern
  
---

## 🔄 Fluxo de Dados

```mermaid
graph TD
    User((Colaborador)) -->|Acessa Portal Público| A[Preenche Formulário]
    A --> DB[(Banco de Dados)]
    
    Admin((Gestor HSE)) -->|Configura| B[Parametrização de Perguntas]
    B -->|Atualiza| A
    
    DB --> C[Dashboard Admin]
    C -->|Gera| D[Relatórios & KPIs]
    D -->|Suporta| E[Ações Preventivas]
