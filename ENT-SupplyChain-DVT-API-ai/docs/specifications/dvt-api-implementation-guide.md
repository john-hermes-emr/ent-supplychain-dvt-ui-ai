# 1. DVT Application: API Implementation Guide

## 1.1. Introduction

### 1.1.1. Purpose
This document specifies the technical implementation details for the development of the DVT API.  
The API consists of multiple components that work together to satisfy the requirements the application must fulfill.  

### 1.1.2. Scope
This document will concentrate on the technical implementation details for the backend API which will be developed using Microsoft the .NET Web API tech stack.

### 1.1.3. Intended Audience
This technical document is designed for software developers, engineers, and technical stakeholders who possess a foundational to advanced understanding of programming concepts, software architecture, and development workflows. The audience may include backend and frontend developers, DevOps professionals, QA engineers, and technical leads who are involved in the design, implementation, testing, and maintenance of software systems. Familiarity with the relevant programming languages, development tools, and system environments discussed in this document is assumed. While the content is primarily technical, it may also be useful to product managers or technical writers seeking deeper insight into the software's functionality and design rationale.  

## Table of Contents <!-- omit from toc -->
- [1. DVT Application: API Implementation Guide](#%31%2E%2D%64%76%74%2D%61%70%70%6C%69%63%61%74%69%6F%6E%3A%2D%61%70%69%2D%69%6D%70%6C%65%6D%65%6E%74%61%74%69%6F%6E%2D%67%75%69%64%65)
  - [1.1. Introduction](#%31%2E%31%2E%2D%69%6E%74%72%6F%64%75%63%74%69%6F%6E)
    - [1.1.1. Purpose](#%31%2E%31%2E%31%2E%2D%70%75%72%70%6F%73%65)
    - [1.1.2. Scope](#%31%2E%31%2E%32%2E%2D%73%63%6F%70%65)
    - [1.1.3. Intended Audience](#%31%2E%31%2E%33%2E%2D%69%6E%74%65%6E%64%65%64%2D%61%75%64%69%65%6E%63%65)
  - [1.2. API Controllers](#%31%2E%32%2E%2D%61%70%69%2D%63%6F%6E%74%72%6F%6C%6C%65%72%73)
    - [1.2.1. \[F\] Job Controller](#%31%2E%32%2E%31%2E%2D%5C%5B%66%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.2.1.1. \[U\] Job Controller - Get Active Job](#%31%2E%32%2E%31%2E%31%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%61%63%74%69%76%65%2D%6A%6F%62)
      - [1.2.1.2. \[U\] Job Controller - Create Job](#%31%2E%32%2E%31%2E%32%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%63%72%65%61%74%65%2D%6A%6F%62)
      - [1.2.1.3. \[U\] Job Controller - Get Job Status](#%31%2E%32%2E%31%2E%33%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%6A%6F%62%2D%73%74%61%74%75%73)
      - [1.2.1.4. \[U\] Job Controller - Validate Files](#%31%2E%32%2E%31%2E%34%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%66%69%6C%65%73)
      - [1.2.1.5. \[U\] Job Controller - Accept Validation Result](#%31%2E%32%2E%31%2E%35%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%61%63%63%65%70%74%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%72%65%73%75%6C%74)
      - [1.2.1.6. \[U\] Job Controller - Generate Output File](#%31%2E%32%2E%31%2E%36%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%6E%65%72%61%74%65%2D%6F%75%74%70%75%74%2D%66%69%6C%65)
      - [1.2.1.7. \[U\] Job Controller - Generate Validation Report](#%31%2E%32%2E%31%2E%37%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%6E%65%72%61%74%65%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%72%65%70%6F%72%74)
    - [1.2.2. \[F\] Storage Controller](#%31%2E%32%2E%32%2E%2D%5C%5B%66%5C%5D%2D%73%74%6F%72%61%67%65%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.2.2.1. \[U\] Storage Controller - Get Folders by Email](#%31%2E%32%2E%32%2E%31%2E%2D%5C%5B%75%5C%5D%2D%73%74%6F%72%61%67%65%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%66%6F%6C%64%65%72%73%2D%62%79%2D%65%6D%61%69%6C)
    - [1.2.3. \[F\] Master Data Controller](#%31%2E%32%2E%33%2E%2D%5C%5B%66%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.2.3.1. \[U\] Master Data Controller - Get Division List](#%31%2E%32%2E%33%2E%31%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%64%69%76%69%73%69%6F%6E%2D%6C%69%73%74)
    - [1.2.4. \[F\] Option List Controller](#%31%2E%32%2E%34%2E%2D%5C%5B%66%5C%5D%2D%6F%70%74%69%6F%6E%2D%6C%69%73%74%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.2.4.1. \[U\] Option List Controller - TBD](#%31%2E%32%2E%34%2E%31%2E%2D%5C%5B%75%5C%5D%2D%6F%70%74%69%6F%6E%2D%6C%69%73%74%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%74%62%64)
    - [1.2.5. \[F\] UserInfo Controller](#%31%2E%32%2E%35%2E%2D%5C%5B%66%5C%5D%2D%75%73%65%72%69%6E%66%6F%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.2.5.1. \[U\] UserInfo Controller - Get basic user information](#%31%2E%32%2E%35%2E%31%2E%2D%5C%5B%75%5C%5D%2D%75%73%65%72%69%6E%66%6F%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%62%61%73%69%63%2D%75%73%65%72%2D%69%6E%66%6F%72%6D%61%74%69%6F%6E)
      - [1.2.5.2. \[U\] UserInfo Controller - Update User Paths](#%31%2E%32%2E%35%2E%32%2E%2D%5C%5B%75%5C%5D%2D%75%73%65%72%69%6E%66%6F%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%75%70%64%61%74%65%2D%75%73%65%72%2D%70%61%74%68%73)
    - [1.2.6. \[F\] About Controller](#%31%2E%32%2E%36%2E%2D%5C%5B%66%5C%5D%2D%61%62%6F%75%74%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.2.6.1. \[U\] About Controller - Base Setup](#%31%2E%32%2E%36%2E%31%2E%2D%5C%5B%75%5C%5D%2D%61%62%6F%75%74%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%62%61%73%65%2D%73%65%74%75%70)
    - [1.2.7. \[F\] Analysis Controller](#%31%2E%32%2E%37%2E%2D%5C%5B%66%5C%5D%2D%61%6E%61%6C%79%73%69%73%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.2.7.1. \[U\] Analysis Controller - Get errors per file](#%31%2E%32%2E%37%2E%31%2E%2D%5C%5B%75%5C%5D%2D%61%6E%61%6C%79%73%69%73%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%65%72%72%6F%72%73%2D%70%65%72%2D%66%69%6C%65)
      - [1.2.7.2. \[U\] Analysis Controller - Get errors report per file](#%31%2E%32%2E%37%2E%32%2E%2D%5C%5B%75%5C%5D%2D%61%6E%61%6C%79%73%69%73%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%65%72%72%6F%72%73%2D%72%65%70%6F%72%74%2D%70%65%72%2D%66%69%6C%65)
      - [1.2.7.3. \[U\] Analysis Controller - Get statistics for job](#%31%2E%32%2E%37%2E%33%2E%2D%5C%5B%75%5C%5D%2D%61%6E%61%6C%79%73%69%73%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%73%74%61%74%69%73%74%69%63%73%2D%66%6F%72%2D%6A%6F%62)
      - [1.2.7.4. \[U\] Analysis Controller - Get statistics for job file](#%31%2E%32%2E%37%2E%34%2E%2D%5C%5B%75%5C%5D%2D%61%6E%61%6C%79%73%69%73%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%73%74%61%74%69%73%74%69%63%73%2D%66%6F%72%2D%6A%6F%62%2D%66%69%6C%65)
      - [1.2.7.5. \[U\] Analysis Controller - Get statistics report for job file](#%31%2E%32%2E%37%2E%35%2E%2D%5C%5B%75%5C%5D%2D%61%6E%61%6C%79%73%69%73%2D%63%6F%6E%74%72%6F%6C%6C%65%72%2D%2D%2D%67%65%74%2D%73%74%61%74%69%73%74%69%63%73%2D%72%65%70%6F%72%74%2D%66%6F%72%2D%6A%6F%62%2D%66%69%6C%65)
  - [1.3. API Contracts](#%31%2E%33%2E%2D%61%70%69%2D%63%6F%6E%74%72%61%63%74%73)
    - [1.3.1. General Contracts](#%31%2E%33%2E%31%2E%2D%67%65%6E%65%72%61%6C%2D%63%6F%6E%74%72%61%63%74%73)
    - [1.3.2. Job Contracts](#%31%2E%33%2E%32%2E%2D%6A%6F%62%2D%63%6F%6E%74%72%61%63%74%73)
    - [1.3.3. Validation Contracts](#%31%2E%33%2E%33%2E%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%63%6F%6E%74%72%61%63%74%73)
  - [1.4. API Services](#%31%2E%34%2E%2D%61%70%69%2D%73%65%72%76%69%63%65%73)
    - [1.4.1. \[F\] Job Service](#%31%2E%34%2E%31%2E%2D%5C%5B%66%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65)
      - [1.4.1.1. \[U\] Job Service - Create basic structure](#%31%2E%34%2E%31%2E%31%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%63%72%65%61%74%65%2D%62%61%73%69%63%2D%73%74%72%75%63%74%75%72%65)
      - [1.4.1.2. \[U\] Job Service - Create a job](#%31%2E%34%2E%31%2E%32%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%63%72%65%61%74%65%2D%61%2D%6A%6F%62)
      - [1.4.1.3. \[U\] Job Service - Update a job](#%31%2E%34%2E%31%2E%33%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%75%70%64%61%74%65%2D%61%2D%6A%6F%62)
      - [1.4.1.4. \[U\] Job Service - Delete a job](#%31%2E%34%2E%31%2E%34%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%64%65%6C%65%74%65%2D%61%2D%6A%6F%62)
      - [1.4.1.5. \[U\] Job Service - Retrieve an active job](#%31%2E%34%2E%31%2E%35%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%72%65%74%72%69%65%76%65%2D%61%6E%2D%61%63%74%69%76%65%2D%6A%6F%62)
      - [1.4.1.6. \[U\] Job Service - Refresh Process](#%31%2E%34%2E%31%2E%36%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%72%65%66%72%65%73%68%2D%70%72%6F%63%65%73%73)
      - [1.4.1.7. \[U\] Job Service - Status management](#%31%2E%34%2E%31%2E%37%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%73%74%61%74%75%73%2D%6D%61%6E%61%67%65%6D%65%6E%74)
      - [1.4.1.8. \[U\] Job Service - Orchestration](#%31%2E%34%2E%31%2E%38%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%6F%72%63%68%65%73%74%72%61%74%69%6F%6E)
      - [1.4.1.9. \[U\] Job Service - Error Logging](#%31%2E%34%2E%31%2E%39%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%65%72%72%6F%72%2D%6C%6F%67%67%69%6E%67)
      - [1.4.1.10. \[U\] Job Service - Create Output File](#%31%2E%34%2E%31%2E%31%30%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%63%72%65%61%74%65%2D%6F%75%74%70%75%74%2D%66%69%6C%65)
      - [1.4.1.11. \[U\] Job Service - Copy Output Files to Supply Chain Folder](#%31%2E%34%2E%31%2E%31%31%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%63%6F%70%79%2D%6F%75%74%70%75%74%2D%66%69%6C%65%73%2D%74%6F%2D%73%75%70%70%6C%79%2D%63%68%61%69%6E%2D%66%6F%6C%64%65%72)
      - [1.4.1.12. \[U\] Job Service - User Log File Generation](#%31%2E%34%2E%31%2E%31%32%2E%2D%5C%5B%75%5C%5D%2D%6A%6F%62%2D%73%65%72%76%69%63%65%2D%2D%2D%75%73%65%72%2D%6C%6F%67%2D%66%69%6C%65%2D%67%65%6E%65%72%61%74%69%6F%6E)
    - [1.4.2. \[F\] Notification Service (Nice-to-have)](#%31%2E%34%2E%32%2E%2D%5C%5B%66%5C%5D%2D%6E%6F%74%69%66%69%63%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%28%6E%69%63%65%2D%74%6F%2D%68%61%76%65%29)
      - [1.4.2.1. \[U\] Send emails upon completion of a file load/validation activity.](#%31%2E%34%2E%32%2E%31%2E%2D%5C%5B%75%5C%5D%2D%73%65%6E%64%2D%65%6D%61%69%6C%73%2D%75%70%6F%6E%2D%63%6F%6D%70%6C%65%74%69%6F%6E%2D%6F%66%2D%61%2D%66%69%6C%65%2D%6C%6F%61%64%2F%76%61%6C%69%64%61%74%69%6F%6E%2D%61%63%74%69%76%69%74%79%2E)
      - [1.4.2.2. \[U\] Send emails upon failure of a file load/validation activity.](#%31%2E%34%2E%32%2E%32%2E%2D%5C%5B%75%5C%5D%2D%73%65%6E%64%2D%65%6D%61%69%6C%73%2D%75%70%6F%6E%2D%66%61%69%6C%75%72%65%2D%6F%66%2D%61%2D%66%69%6C%65%2D%6C%6F%61%64%2F%76%61%6C%69%64%61%74%69%6F%6E%2D%61%63%74%69%76%69%74%79%2E)
      - [1.4.2.3. \[U\] Ability to opt in or out of notifications per user.](#%31%2E%34%2E%32%2E%33%2E%2D%5C%5B%75%5C%5D%2D%61%62%69%6C%69%74%79%2D%74%6F%2D%6F%70%74%2D%69%6E%2D%6F%72%2D%6F%75%74%2D%6F%66%2D%6E%6F%74%69%66%69%63%61%74%69%6F%6E%73%2D%70%65%72%2D%75%73%65%72%2E)
    - [1.4.3. \[F\] File Load Service (File Load API)](#%31%2E%34%2E%33%2E%2D%5C%5B%66%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%28%66%69%6C%65%2D%6C%6F%61%64%2D%61%70%69%29)
      - [1.4.3.1. \[U\] File Load Service - Base Components](#%31%2E%34%2E%33%2E%31%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%62%61%73%65%2D%63%6F%6D%70%6F%6E%65%6E%74%73)
      - [1.4.3.2. \[U\] File Load Service - Vir File Loader](#%31%2E%34%2E%33%2E%32%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%76%69%72%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
      - [1.4.3.3. \[U\] File Load Service - Inventory File Loader](#%31%2E%34%2E%33%2E%33%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%69%6E%76%65%6E%74%6F%72%79%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
      - [1.4.3.4. \[U\] File Load Service - Item File Loader](#%31%2E%34%2E%33%2E%34%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%69%74%65%6D%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
      - [1.4.3.5. \[U\] File Load Service - Supplier File Loader](#%31%2E%34%2E%33%2E%35%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%73%75%70%70%6C%69%65%72%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
      - [1.4.3.6. \[U\] File Load Service - PO File Loader](#%31%2E%34%2E%33%2E%36%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%70%6F%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
      - [1.4.3.7. \[U\] File Load Service - PO Item File Loader](#%31%2E%34%2E%33%2E%37%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%70%6F%2D%69%74%65%6D%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
      - [1.4.3.8. \[U\] File Load Service - UOM File Loader](#%31%2E%34%2E%33%2E%38%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%75%6F%6D%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
      - [1.4.3.9. \[U\] File Load Service - MPN File Loader](#%31%2E%34%2E%33%2E%39%2E%2D%5C%5B%75%5C%5D%2D%66%69%6C%65%2D%6C%6F%61%64%2D%73%65%72%76%69%63%65%2D%2D%2D%6D%70%6E%2D%66%69%6C%65%2D%6C%6F%61%64%65%72)
    - [1.4.4. \[F\] Master Data Service](#%31%2E%34%2E%34%2E%2D%5C%5B%66%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65)
      - [1.4.4.1. \[U\] Master Data Service - List of Master Data](#%31%2E%34%2E%34%2E%31%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%6C%69%73%74%2D%6F%66%2D%6D%61%73%74%65%72%2D%64%61%74%61)
      - [1.4.4.2. \[U\] Master Data Service - Verify Division](#%31%2E%34%2E%34%2E%32%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%76%65%72%69%66%79%2D%64%69%76%69%73%69%6F%6E)
      - [1.4.4.3. \[U\] Master Data Service - Verify Site](#%31%2E%34%2E%34%2E%33%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%76%65%72%69%66%79%2D%73%69%74%65)
      - [1.4.4.4. \[U\] Master Data Service - Verify UOM](#%31%2E%34%2E%34%2E%34%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%76%65%72%69%66%79%2D%75%6F%6D)
      - [1.4.4.5. \[U\] Master Data Service - Verify Currency](#%31%2E%34%2E%34%2E%35%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%76%65%72%69%66%79%2D%63%75%72%72%65%6E%63%79)
      - [1.4.4.6. \[U\] Master Data Service - Verify Commodity](#%31%2E%34%2E%34%2E%36%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%76%65%72%69%66%79%2D%63%6F%6D%6D%6F%64%69%74%79)
      - [1.4.4.7. \[U\] Master Data Service - Verify Payment Terms](#%31%2E%34%2E%34%2E%37%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%76%65%72%69%66%79%2D%70%61%79%6D%65%6E%74%2D%74%65%72%6D%73)
      - [1.4.4.8. \[U\] Master Data Service - Verify Freight Terms](#%31%2E%34%2E%34%2E%38%2E%2D%5C%5B%75%5C%5D%2D%6D%61%73%74%65%72%2D%64%61%74%61%2D%73%65%72%76%69%63%65%2D%2D%2D%76%65%72%69%66%79%2D%66%72%65%69%67%68%74%2D%74%65%72%6D%73)
    - [1.4.5. \[F\] Storage Service](#%31%2E%34%2E%35%2E%2D%5C%5B%66%5C%5D%2D%73%74%6F%72%61%67%65%2D%73%65%72%76%69%63%65)
      - [1.4.5.1. \[U\] Storage Service - File Retrieval](#%31%2E%34%2E%35%2E%31%2E%2D%5C%5B%75%5C%5D%2D%73%74%6F%72%61%67%65%2D%73%65%72%76%69%63%65%2D%2D%2D%66%69%6C%65%2D%72%65%74%72%69%65%76%61%6C)
      - [1.4.5.2. \[U\] Storage Service - Job Directory Operations](#%31%2E%34%2E%35%2E%32%2E%2D%5C%5B%75%5C%5D%2D%73%74%6F%72%61%67%65%2D%73%65%72%76%69%63%65%2D%2D%2D%6A%6F%62%2D%64%69%72%65%63%74%6F%72%79%2D%6F%70%65%72%61%74%69%6F%6E%73)
      - [1.4.5.3. \[U\] Storage Service - Create File](#%31%2E%34%2E%35%2E%33%2E%2D%5C%5B%75%5C%5D%2D%73%74%6F%72%61%67%65%2D%73%65%72%76%69%63%65%2D%2D%2D%63%72%65%61%74%65%2D%66%69%6C%65)
    - [1.4.6. \[F\] Log Service](#%31%2E%34%2E%36%2E%2D%5C%5B%66%5C%5D%2D%6C%6F%67%2D%73%65%72%76%69%63%65)
      - [1.4.6.1. \[U\] Log Service - Create a Log Entry](#%31%2E%34%2E%36%2E%31%2E%2D%5C%5B%75%5C%5D%2D%6C%6F%67%2D%73%65%72%76%69%63%65%2D%2D%2D%63%72%65%61%74%65%2D%61%2D%6C%6F%67%2D%65%6E%74%72%79)
    - [1.4.7. \[F\] User Service](#%31%2E%34%2E%37%2E%2D%5C%5B%66%5C%5D%2D%75%73%65%72%2D%73%65%72%76%69%63%65)
      - [1.4.7.1. \[U\] User Service - Get user information](#%31%2E%34%2E%37%2E%31%2E%2D%5C%5B%75%5C%5D%2D%75%73%65%72%2D%73%65%72%76%69%63%65%2D%2D%2D%67%65%74%2D%75%73%65%72%2D%69%6E%66%6F%72%6D%61%74%69%6F%6E)
      - [1.4.7.2. \[U\] User Service - Persist Log, Load, and Output path selections](#%31%2E%34%2E%37%2E%32%2E%2D%5C%5B%75%5C%5D%2D%75%73%65%72%2D%73%65%72%76%69%63%65%2D%2D%2D%70%65%72%73%69%73%74%2D%6C%6F%67%2C%2D%6C%6F%61%64%2C%2D%61%6E%64%2D%6F%75%74%70%75%74%2D%70%61%74%68%2D%73%65%6C%65%63%74%69%6F%6E%73)
    - [1.4.8. \[F\] Output File Service](#%31%2E%34%2E%38%2E%2D%5C%5B%66%5C%5D%2D%6F%75%74%70%75%74%2D%66%69%6C%65%2D%73%65%72%76%69%63%65)
      - [1.4.8.1. \[U\] Output File Service - Create Output File](#%31%2E%34%2E%38%2E%31%2E%2D%5C%5B%75%5C%5D%2D%6F%75%74%70%75%74%2D%66%69%6C%65%2D%73%65%72%76%69%63%65%2D%2D%2D%63%72%65%61%74%65%2D%6F%75%74%70%75%74%2D%66%69%6C%65)
    - [1.4.9. \[F\] Validation Service](#%31%2E%34%2E%39%2E%2D%5C%5B%66%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65)
      - [1.4.9.1. Performance Note on Validation Data Storage](#%31%2E%34%2E%39%2E%31%2E%2D%70%65%72%66%6F%72%6D%61%6E%63%65%2D%6E%6F%74%65%2D%6F%6E%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%64%61%74%61%2D%73%74%6F%72%61%67%65)
      - [1.4.9.2. \[U\] Validation Service - Validate Vir File](#%31%2E%34%2E%39%2E%32%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%76%69%72%2D%66%69%6C%65)
  - [1.5. Acceptance Criteria](#%31%2E%35%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.5.0.1. \[U\] Validation Service - Validate Inventory File](#%31%2E%35%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%69%6E%76%65%6E%74%6F%72%79%2D%66%69%6C%65)
  - [1.6. Acceptance Criteria](#%31%2E%36%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.6.0.1. \[U\] Validation Service - Validate Item File](#%31%2E%36%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%69%74%65%6D%2D%66%69%6C%65)
  - [1.7. Acceptance Criteria](#%31%2E%37%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.7.0.1. \[U\] Validation Service - Validate Supplier File](#%31%2E%37%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%73%75%70%70%6C%69%65%72%2D%66%69%6C%65)
  - [1.8. Acceptance Criteria](#%31%2E%38%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.8.0.1. \[U\] Validation Service - Validate PO File](#%31%2E%38%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%70%6F%2D%66%69%6C%65)
  - [1.9. Acceptance Criteria](#%31%2E%39%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.9.0.1. \[U\] Validation Service - Validate PO Item File](#%31%2E%39%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%70%6F%2D%69%74%65%6D%2D%66%69%6C%65)
  - [1.10. Acceptance Criteria](#%31%2E%31%30%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.10.0.1. \[U\] Validation Service - Validate UOM File](#%31%2E%31%30%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%75%6F%6D%2D%66%69%6C%65)
  - [1.11. Acceptance Criteria](#%31%2E%31%31%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.11.0.1. \[U\] Validation Service - Validate MPN File](#%31%2E%31%31%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%65%2D%6D%70%6E%2D%66%69%6C%65)
  - [1.12. Acceptance Criteria](#%31%2E%31%32%2E%2D%61%63%63%65%70%74%61%6E%63%65%2D%63%72%69%74%65%72%69%61)
      - [1.12.0.1. \[U\] Validation Service - Generate statistics Report for Analysis Controller](#%31%2E%31%32%2E%30%2E%31%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%67%65%6E%65%72%61%74%65%2D%73%74%61%74%69%73%74%69%63%73%2D%72%65%70%6F%72%74%2D%66%6F%72%2D%61%6E%61%6C%79%73%69%73%2D%63%6F%6E%74%72%6F%6C%6C%65%72)
      - [1.12.0.2. \[U\] Validation Service - Validation Message Structure](#%31%2E%31%32%2E%30%2E%32%2E%2D%5C%5B%75%5C%5D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%73%65%72%76%69%63%65%2D%2D%2D%76%61%6C%69%64%61%74%69%6F%6E%2D%6D%65%73%73%61%67%65%2D%73%74%72%75%63%74%75%72%65)
  - [1.13. Priority List](#%31%2E%31%33%2E%2D%70%72%69%6F%72%69%74%79%2D%6C%69%73%74)
  - [1.14. Useful Links](#%31%2E%31%34%2E%2D%75%73%65%66%75%6C%2D%6C%69%6E%6B%73)



## 1.2. API Controllers  

### 1.2.1. \[F\] Job Controller 
**Description**  
The Job Controller will allow the UI to perform all the job-related activities necessary to successfully load, validate and save files.

**Benefits Hypothesis**  
Providing a single-point of entry into the system for the DVT UI will allow the system to main as much of the business logic as possible within the API.  
Reducing the decision points and different controllers the UI has to work with will minimize the effort on the UI and will allow us to be able to test much of the functionality of the system with API unit tests.

**Acceptance Criteria**
- Create the following endpoints:
  - GetActiveJob - Checks for any active jobs created by the given user email and returns it back to the UI.
  - CreateJob - Creates a new job for the user to perform a file upload/validation.   
  - GetJobStatus - Returns the status of the job and all files within the job    
  - ValidateFiles - Starts the validation process for one or more files in the job  
  - AcceptValidationResult - Marks a file in the job as accepted and ready to produce output.
  - GenerateOutput - Creates the output files for the given job that are marked as ACCEPTED.  
      

#### 1.2.1.1. \[U\] Job Controller - Get Active Job
**Description:**
 Checks for any active jobs created by the given user email and returns it back to the UI.

**Acceptance Criteria**
- Create an endpoint called GetActiveJob
  - Endpoint shall only be available to the user who is logged in. 
  - Cannot get active jobs from a different user than the user who is logged in.
  - Use the following API route: /jobs/get-active/user-id/{id}
  - Parameters: UserEmail - The email of the user logged into the application
  - Return Payload: OperationResult


#### 1.2.1.2. \[U\] Job Controller - Create Job
**Description:**
Creates a new job for the user to perform a file upload/validation.

**Acceptance Criteria**
- Create an endpoint called CreateJob
  - Endpoint shall only be available to the user who is logged in.   - 
  - Use the following API route: /jobs/
  - Payload: JobCreationRequest
  - Parameters: JobCreationRequest  
    - UserId: The id of the user creating the job.
    - DivisionId: The selected division id
    - FeedNumber: The selected feed number
  - Return Payload: OperationResult of Job


#### 1.2.1.3. \[U\] Job Controller - Get Job Status
**Description:**
Returns the status of the job and all files within the job

**Acceptance Criteria**
- Create an endpoint called GetJobStatus
  - Endpoint shall only be available to the user who is logged in. 
  - Cannot get the job status of a job from a different user than the user who is logged in.
  - Use the following API route: /jobs/status/{id}
  - Parameters: JobId
    - Return Payload: OperationResult of JobStatus


#### 1.2.1.4. \[U\] Job Controller - Validate Files
**Description:**
Starts the validation process for one or more files in the job

**Acceptance Criteria**
- Create an endpoint called ValidateFiles
  - Endpoint shall only be available to the user who is logged in. 
  - Cannot get the file share listings from a different user than the user who is logged in.
  - Use the following API route: /jobs/validate-files/job/{id}
  - Parameters: JobValidationRequest
    - JobId: The Id of the job being validated.
    - SelectedFileIds: The list of file IDs the user has selected to validate
  - Return Payload: OperationResult of JobStatus


#### 1.2.1.5. \[U\] Job Controller - Accept Validation Result
**Description:**
Marks a file in the job as accepted and ready to produce output.

**Acceptance Criteria**
- Create an endpoint called AcceptValidationResult
  - Endpoint shall only be available to the user who is logged in. 
  - Cannot get the file share listings from a different user than the user who is logged in.
  - Use the following API route: /jobs/validation-accept/job-file/{id}
  - Parameters: AcceptJobFileRequest
    - JobId: The Id of the job being worked on
    - JobFileId: The Id of the file being accepted.
  - Return Payload: OperationResult of JobStatus


#### 1.2.1.6. \[U\] Job Controller - Generate Output File
**Description:**
Creates the output files for the given job that are marked as ACCEPTED.
This is called by the UI when the user clicks on the **Complete** button.

**Acceptance Criteria**
- Create an endpoint called GenerateOutputFile
  - Endpoint shall only be available to the user who is logged in. 
  - Cannot get the file share listings from a different user than the user who is logged in.
  - Use the following API route: /jobs/output-file/job-file/{id}
  - Parameters: GenerateOutputFileRequest
      - JobId: The Id of the job being worked on
    - Return Payload: OperationResult of JobStatus

#### 1.2.1.7. \[U\] Job Controller - Generate Validation Report
**Description:**
Creates a report version of the validation results for the given job that are marked as ACCEPTED.

**Acceptance Criteria**
- Create an endpoint called GenerateValidationReport
  - Endpoint shall only be available to the user who is logged in. 
  - Cannot get the file share listings from a different user than the user who is logged in.
  - Use the following API route: /jobs/validation-report/job-file/{id}
  - Parameters: GenerateOutputFileRequest
      - JobId: The Id of the job being worked on
    - Return Payload: OperationResult of JobStatus


### 1.2.2. \[F\] Storage Controller  
**Description** 
The Storage Controller will allow the UI to interact with the application's storage directly.  DVT will utilize an Azure File Share where all data is stored and handled which includes the user's home and log folder. The user must have the ability to select a folder to be their dedicated home and log folder and the Storage Controller will serve that purpose.

**Benefits Hypothesis**  
We believe that implementing a Storage Controller for direct UI-to-storage interaction
Will result in improved user experience and streamlined data management
Because users will have direct control over their file organization and storage locations, leading to faster file operations, reduced dependency on backend services, and enhanced workflow efficiency.

**Acceptance Criteria**
- Note: Rename the File Controller
- Create the following endpoints:
  - GetFoldersByEmailAddress - Returns a listing of all folders available in the user's file share given the user's email address.
  - GetFileDownloadUrl - Returns the download URL for a specific file given the file name.  

 #### 1.2.2.1. \[U\] Storage Controller - Get Folders by Email
**Description:**
 As the Storage Controller, I would like to give the UI the ability to get the listing of folders in a particular user's file share given their email address so the user can pick which folder they wish to use as their load or log folder.

**Acceptance Criteria:**
- Create an endpoint called GetFoldersFromUserSharebyEmailAddress
  - Endpoint shall only be available to the user who is logged in. 
  - Cannot get the file share listings from a different user than the user who is logged in.
  - Use the following API route: /storage/folder-list/email-address/{id}

### 1.2.3. \[F\] Master Data Controller
**Description**  
The master data controller will allow the UI to get a listing of various master data entities. Currently, there is only the need to return the list of Divisions in DVT. In the future, we may expand the functionality of the Master Data Controller.

**Benefits Hypothesis**  
The UI requires a listing of the Divisions available in DVT. To maintain separation of concerns within the application, the master data shall only be interacted with through the master data controller.

**Acceptance Criteria**
- Return the list of Divisions to the user interface.

#### 1.2.3.1. \[U\] Master Data Controller - Get Division List
**Description:**  
To begin the file upload and validation process the user must select a division from the available list of divisions in DVT. 
The Master data controller shall provide the ability for UI to get a list of master data.

**Acceptance Criteria:**
- Create an endpoint to retrieve the list divisions in DVT
  - Endpoint shall be available to any logged-in user.
  - Endpoint API route: /master-data/divisions/
  - Division list shall be sorted by division id ascending and division name alphabetically
  - The return payload will consist of the following fields:
    - Id - UUID identifier for each division given that DivisionId repeats
    - DivisionId - Integer representing division ID from source system.
    - DivisionAbbreviation - Shortened version of Division
    - Division - The name of the division


### 1.2.4. \[F\] Option List Controller
**Description**  
TBD - We don't have any lists to be served at the moment
**Benefits Hypothesis**  
TBD - We don't have any lists to be served at the moment
**Acceptance Criteria**
TBD - We don't have any lists to be served at the moment

#### 1.2.4.1. \[U\] Option List Controller - TBD
**Description:**  
TBD - We don't have any lists to be served at the moment

**Acceptance Criteria:**
TBD - We don't have any lists to be served at the moment


### 1.2.5. \[F\] UserInfo Controller
**Description**  
The User Info Controller will allow the user interface to get information about the current user logged into the application.

**Benefits Hypothesis**  
Consolidating all the user info-related functionality into a single controller will promote separation of concerns.

**Acceptance Criteria**
- Provide the basic user information to the UI given:
  - The user's ID
  - The user's email address

#### 1.2.5.1. \[U\] UserInfo Controller - Get basic user information
**Description:**  
Basic user information shall consist of the following fields:
- User ID
- First Name
- Last Name
- Email Address
  
**Acceptance Criteria:**
- Create an endpoint to return the user's information by user ID
  - Endpoint shall be available to any logged-in user.
  - Endpoint API route: /user-info/{id}
  
- Create an endpoint to return the user's information by email address
  - Endpoint shall be available to any logged-in user.
  - Endpoint API route: /user-info/email-address/{id}

#### 1.2.5.2. \[U\] UserInfo Controller - Update User Paths
**Description:**  
The UserInfoController will provide endpoints to allow the user to save their load, log and output paths

**Acceptance Criteria:**
- Create an endpoint that will accept the user's load folder, log folder and output folder paths
  - The endpoint shall only be available to authenticated users
  - Endpoint API route: /user-info
  - Endpoint shall return a JSON response payload with the following fields
    - UserInfoId: User identifier in the system
    - FirstName: User's first name
    - LastName: User's last name
    - EmailAddress: The user's email address stored in the system
    - LoadDirectory: The path to the user's load folder
    - LogDirectory: The path to the user's log folder
    - OutputDirectory: The path to the user's output directory


### 1.2.6. \[F\] About Controller
**Description**  
The about controller is the most simple way of checking if the API has been deployed properly and it's running.
It returns some basic information as the name of the application and the date.

**Benefits Hypothesis**  
Having an About Controller allows the development and support team to quickly find out if the API is running.

**Acceptance Criteria**
- Create an endpoint with basic information that can be called by anyone.

#### 1.2.6.1. \[U\] About Controller - Base Setup
**Description:**  
The about controller is the most simple way of checking if the API has been deployed propery and it's running.
It returns some basic information as the name of the application and the date.

**Acceptance Criteria:**
- Create an endpoint that will return basic information about the API
  - The endpoint shall be available to anyone including unauthenticated users.
  - Endpoint API route: /about
  - Endpoint shall return a JSON response payload with two fields aboutTime and aboutMessage
    - AboutTime: DOW, DD MM YYYY HH:MM:SS AMPM in UTC time. Example: Tuesday, 29 July 2025 11:49:00 AM
    - AboutMessage: DVT Application Core API. DVT (Data Validation Tool) is an application that loads and validates Direct Material Operational data from Procurement, Accounts Payable and MRP/ERP systems for use in Oracle Fusion Analytic Warehouse (FAW) instance.


### 1.2.7. \[F\] Analysis Controller
**Description**  
The Analysis Controller allows the UI to get a summary of all the issues discovered during file validation.
In addition, the controller provides endpoints to get useful statistical information about the last set of files that were uploaded and validated so they can be displayed to the user.

**Technical Note:** See the API Contracts section for a more detailed description of the expected payloads.

**Benefits Hypothesis**  
Implementing the Analysis Controller Will result in improved user decision-making and faster issue resolution.
Because users will have immediate access to comprehensive validation results, error summaries, and file statistics, enabling them to quickly identify data quality issues, understand the scope of problems, and take corrective actions without manually reviewing individual files.

**Acceptance Criteria**
- Get Analysis Errors per file
- Get Analysis statistics for job 

#### 1.2.7.1. \[U\] Analysis Controller - Get errors per file
**Description:**  
The Analysis Controller shall have an endpoint that will allow the UI to get the error details for a file selected in the main grid.
The API shall return the information in a structure that will allow the UI to easily display the information to the user without much manipulation.

**Acceptance Criteria**
- Create an endpoint to request error information given a JobId and JobFileId
  - The endpoint shall be available to any logged-in user.
  - Endpoint Route: /analysis/errors/
  - Endpoint request shall contain the JobId and JobFileId
  - The error response payload shall be in JSON format
  - The response payload shall contain:
    - Basic information about the file.
    - Summarized listing of the count of errors grouped by Message Type (WARNING, ERRORS, CRITICAL) and Field
    - Detailed row-by-row summary of the errors found in the file.
- Create an endpoint to request the error report for a given JobId and JobFileId
  - The endpoint shall be available to any logged-in user.
  - Endpoint Route: /analysis/error-report/
  - Endpoint request shall contain the JobId and JobFileId
  - The error response payload shall be in JSON format
  - The response payload shall contain:
    - Header Information: 
      - Date: MM/DD/YYYY
      - Filename: xxx.txt
    - For each row that contains an error. The text block below shall be displayed followed by 2 blank lines.
      - Row Number: |record #|
      - Problem: Status Message
      - Validation Messages: |Validation messages for the row|
      - Data: |Attempted value which caused a fail in validation|
        - Note: For errors that are not specific to a particular column such as header fields, we shall output a dash (-)
        - Note: For values that are longer than 10 characters, we will display the first 10 characters followed by ...
  

#### 1.2.7.2. \[U\] Analysis Controller - Get errors report per file
**Description:**  
The Analysis Controller shall have an endpoint that will allow the UI to get the error details as an excel file for a file selected in the main grid.
The API shall return the information formatted as an excel file that will get saved to the user's default download folder.

**Acceptance Criteria**
- Crate an endpoint to request error information for a job file given the JobId and JobFileId.
- The endpoint shall be available to any logged-in user.
  - Endpoint Route: /analysis/error-report (request payload: GetFileValidationResultRequest)
    - RequestPayload: GetFileValidationResultRequest(JobId, JobFileId)
  - The error response payload shall be in xlsx format
- The response payload shall contain:
  - Basic information about the file.
  - List of statistical information for the selected file in the job.

#### 1.2.7.3. \[U\] Analysis Controller - Get statistics for job
**Description:**  
The Analysis Controller shall allow the user to get a listing of basic information about the files validated for a given job. As a job may contain multiple files, the response payload will contain all the information available for the files in the job.

**Acceptance Criteria**
- Create an endpoint to request statistic information given a JobId
  - The endpoint shall be available to any logged-in user.
  - Endpoint Route: /analysis/stats/job/{id}  
  - The error response payload shall be in JSON format
- The response payload shall contain:
  - Basic information about the file.
  - List of statistical information for all the files in the job.

#### 1.2.7.4. \[U\] Analysis Controller - Get statistics for job file
**Description:**  
The Analysis Controller shall allow the user to get a listing of basic information about a particular file validated for a given job. The response payload will contain all the information available for the selected file in the job.

**Acceptance Criteria**
- Create an endpoint to request statistic information given a JobId and JobFileId
  - The endpoint shall be available to any logged-in user.
  - Endpoint Route: /analysis/stats/ (Request Payload) 
    - GetFileValidationResultRequest(JobId, JobFileId)
  - The error response payload shall be in JSON format
- The response payload shall contain:
  - Basic information about the file.
  - List of statistical information for the provided file.

#### 1.2.7.5. \[U\] Analysis Controller - Get statistics report for job file
**Description:**  
The Analysis Controller shall allow the user to get a listing of basic information about a particular file validated for a given job. The response payload will contain all the information available for the selected file in the job.

**Acceptance Criteria**
- Create an endpoint to request statistic information given a JobId and JobFileId
  - The endpoint shall be available to any logged-in user.
  - Endpoint Route: /analysis/stats-report/ (Request Payload: GetFileValidationResultRequest) 
    - GetFileValidationResultRequest(JobId, JobFileId)
  - The error response payload shall be in JSON format
- The response payload shall contain:
  - Basic information about the file.
  - List of statistical information for the provided file.

## 1.3. API Contracts
The DVT API will utilize multiple API contracts to communicate with the API. Although a detailed API OPenAPI document will be created, we will provide a high-level description of the most important ones in the following sections.

### 1.3.1. General Contracts
- OperationResult - The OperationResult contract will be used as a generic payload when an operation is completed in the application. The followings fields will be included in the payload:
  - Data -  Type: Object. Contains the main object being returned such as a job, user, etc...
  - Operation - Type: String. This is the name of the operation that triggered the response such as GetActiveJob, SaveJob, etc...
  - ReturnCode - Type: String. Gives further detail about the outcome of the operation such as: COMPLETE, EXCEPTION, etc...
  - Success - Type: Bool. Indicates whether the operation was successful.
  - ErrorMessage - Type: String. Contains any error messages from the operation.
  - Exception - Type: JSON text. Contains any exception information that we may wish to return to the caller.

### 1.3.2. Job Contracts
TODO

### 1.3.3. Validation Contracts
```json
"FileValidationResult":{
  "FileName":"abc_123.txt",
  "ValidationRows":[
    {
      "Row":1,
      "Status":"ERRORS",
      "Columns":[
        {
          "Name":"PO Number",
          "Message":"PO Number is missing"
        },
        {
          "Name":"Item Number",
          "Message":"Item Number must be between 1 and 10 characters. You entered 20 characters." 
        }
      ]
    }
  ]
}
```

## 1.4. API Services  

### 1.4.1. \[F\] Job Service

**Description**  
The Job Service will operate as the central manager for all the operations that happen in the DVT API. The Job service will rely on other services to perform necessary operations to create, execute and complete a job.  

What is a job? - A job is a container of files that the application will operate on. When the user selects a division and feed number from the list, the job service will check for any job templates available with that combination. The job template will contain a list of file types that are expected as part of the job. If a template is not found based on the division and feed number, an error will be returned to the caller. Once a template has been located, the job service will create a job for the user based on the selected template. The file types that are part of the selected template, will be added to the job and saved to the database. After the job is created, it will be returned to the caller as a JSON structure.

At a high-level, the Job Service will oversee the following operations:
- Create a job based on the user's selection of Division and Feed number
- Provide job status
- Initiate and track the loading of all files in the job
- Initiate and track the validation of all files
- Initiate and track the production of output files
- Store the load, validation and output file creation results to the database.

Refer to the links section of this document for a reference to the Lucid diagram which is kept up-to-date. 
In addition, a snapshot of the diagram is available [here](./images/job-service-class-diagram.png)  

**Benefit Hypothesis**  
- By implementing the Job Service as a centralized orchestrator, we can ensure consistent workflow management across all DVT operations, reducing the complexity of inter-service communication and providing a single point of control for job lifecycle management.
- Using template-based job creation allows for standardized file type configurations per division and feed combination, enabling easier maintenance of business rules and faster onboarding of new divisions without code changes.
- The service's delegation pattern to specialized services (File Load, Validation, Output File) promotes separation of concerns and allows for independent scaling and testing of each component, improving overall system reliability and maintainability.
- Centralized job status tracking and result storage provides comprehensive audit trails and enables better monitoring, troubleshooting, and reporting capabilities for business stakeholders.
- The template validation approach ensures data integrity by preventing jobs from being created with incomplete or incorrect file type configurations, reducing downstream processing errors and improving user experience.

**Acceptance Criteria**  
- Implement the following components to cover the functions defined above:
  - Create the necessary table structures and data models to support the job service   
   - FileTemplate
   - Job
   - JobFile
   - JobLog  
- Create the JobService class following the class diagram with methods for:  
  - Getting an active job by user id
  - Create a job
  - Validate files in a job
  - Get job status
  - Update job status
  - Delete a job by Id
  - Create output files for a job
  - Calculate Statistics
- if no template is found return an exception
- Create necessary unit tests to support the functions added. 

#### 1.4.1.1. \[U\] Job Service - Create basic structure
**Description:**  
Create the basic components and classes necessary to support the features of the job service.
This involves creating classes, database tables, repositories, etc...  
The table structure can be found in the lucid diagram linked in the links section.

**Acceptance Criteria:**
- Create required tables from ERD
- Create service classes from architecture diagram
  - JobService
  - JobCreationRequest
  - JobValidationRequest
  - JobStatus
  - OperationResult
- Create required data repositories to support the database tables
- Tables/models to be created
  - JobTemplate
  - FileTemplate
  - Job
  - JobFile
  - JobLog  

#### 1.4.1.2. \[U\] Job Service - Create a job
**Description:**  
Implement the logic to create a job based on the user's input (division, feed number).  
The job will contain a list of JobFile objects which contain the information about the file to be loaded such as which file template corresponds to the file, the status of the file, file name, file path, record count and other properties.  

:::mermaid
erDiagram
    UserInfo ||--o{ Job : Creates
    UserInfo {
        UserId UUID        
    }
    Job ||--|{ JobFile : contains
    Job {
        JobId UUID
        DivisionId UUID
        UserId UUID      
    }
    JobFile {
        JobFileId UUID
        JobId UUID        
    }
:::

When a job is created, the following fields shall be used:

> Job table structure
- JobId: Unique identifier for each job.
- DivisionId: Reference to the Division selected by the user for this job.
- UserId: The identifier of the user that created the job.
- Status: Job status - CREATED, UPLOADED, VALIDATED, COMPLETED
- FeedNumber: Integer that identifies a particular set of files to be loaded.
- ArchiveFilePath: The path in the main share where the zip archive of the job files is located.
- CreatedBy: Email address of user
- CreatedDate: Timestamp of when the job was created.
- UpdatedBy: Email address of the user who updated the job
- UpdateDate: Timestamp of when the job was last updated.

When a job is to be created, the system shall create the necessary job files. Each job file will have the following information: 

> JobFile table structure
- JobFileId: Unique identifier for the database.
- JobId: The ID of the Job parent record.
- FileType: The *unique* string representing the type of file: i.e. Vir, Item, Po
- FilenameFormat: The string representing the pattern that the file must have i.e. *[div abbrev]_[feed number]_vir_o.txt*
- SortOder: The order in which the file will be sorted when displayed in the UI.
- Optional: Flag used to mark if a file is optional or not.
- DependsOnFileType: A CSV string of the file types that this file is dependent on. i.e. Vir file depends on Item and Supplier files
- Filename: The actual filename of the file in the storage location
- FilePath: The full path of the file in the storage location
- Status: The status of the file through the load and validation process. 
    - UPLOADED - file has been read by DVT
    - VALIDATED - validation process contains no errors for all records
    - WARNING - validation process contains warning errors
    - ERRORS - validation process contains errors    
    - CRITICAL - cannot transmit any records due to critical validation error 
    - ACCEPTED - The validation results have been accepted the output can be produced.
    - ACCEPTED-WARNINGS - The validation results have been accepted but the file had warnings the output can be produced.
    - ACCEPTED-ERRORS - The validation results have been accepted but the file had errors the output can be produced.
    - COMPLETED  - The output file has been successfully  created    
- CreationTimestamp: The time the physical file was created
- RecordCount: The total number of records in the file.
- LoadDate: The date in which the file was loaded into DVT.
- ValidationMessages: A JSON structure containing all the validation messages for the file.

Method for create job. This will call Create job file one for every file. hard-coded with the file types, etc...
Method for create job file

| Table          | File Type | Filename Format                           | Sort Order | Optional | Depends On File |
| -------------- | --------- | ----------------------------------------- | ---------- | -------- | --------------- |
| VIRTable       | Vir       | [div abbrev]_[feed number]_vir_o.txt      | 1          | Y        | Supplier,Item   |
| InventoryTable | Inventory | [div abbrev]_[feed number]_inv_o.txt      | 2          | N        | Item            |
| ItemTable      | Item      | [div abbrev]_[feed number]_item_o.txt     | 3          | N        | None            |
| POTable        | Po        | [div abbrev]_[feed number]_po_o.txt       | 4          | N        | Supplier        |
| POItemTable    | PoItem    | [div abbrev]_[feed number]_poitem_o.txt   | 5          | N        | PO              |
| SupplierTable  | Supplier  | [div abbrev]_[feed number]_supplier_o.txt | 6          | N        | None            |
| MPNTable       | Mpn       | [div abbrev]_[feed number]_mpn_o.txt      | 7          | Y        | Item            |
| UOMTable       | Uom       | [div abbrev]_[feed number]_uom_o.txt      | 8          | Y        | Item            |

**Acceptance Criteria:**
- > Given: The user wants to validate files for a division and feed number.
  - When: The JobService receives the division and feed number parameters and the create job action is initiated.
    - Then: The JobService shall create the job including the list of files as devined in the table and return the job back to the caller wrapped in a OperationResult.

#### 1.4.1.3. \[U\] Job Service - Update a job
**Description:**  
As a job progresses from creation, file loading, validation and output file creation, the JobService shall maintain the status of the job up to date in the database so that it may be reported back to the caller when required. The Job contains a log which shall be maintained as the system progresses through the various phases of the job. This will allow the system to provide a step-by-step recall of the events that happened during the job's lifetime.

**Acceptance Criteria:**
- > Given: The system needs to modify a job
  - When: The job service calls the UpdateJob method
    - Then: The modifications made to the job shall be persisted to the database.

- > Given: The system needs to the state of a JobFile and not the entire job.
  - When: The JobService calls the UpdateJobFile method and passes in the a JobFile object
    - Then: The given JobFile shall be updated in the database.

#### 1.4.1.4. \[U\] Job Service - Delete a job
**Description:** 
When a user has created a job but no longer wishes to continue the previous job or wishes to start a new one, the application shall allow the user to delete a previously created job.

**Acceptance Criteria:**
- > Given: The user lands in the main page after having created a previous job and the user has been presented with the "Resume Job" dialog asking the user if they wish to resume, cancel or refresh.
  - When: The user answers "Cancel" when asked if they wish to resume the existing job.
    - Then: The job is deleted from the database and any temporary working files associated with that job shall be deleted.
  - When: The user answers "Resume" when asked if they wish to resume the existing job.
    - Then: The job is loaded into the UI and the dialog box disappears.

#### 1.4.1.5. \[U\] Job Service - Retrieve an active job
**Description:**  
When the application creates a job and the user closes the browser, or navigates away from the page, the system shall allow the user to resume the previously active job so that the user is able to complete the validation and upload task.

**Acceptance Criteria:**
- > Given: The user lands in the main page after having created a previous job and the GetActiveJob method is called given the user's Id.
  - When: There is an active job for the user
    - Then: The job is retrieved from the database and returned to the caller wrapped in a OperationResult object.
  - When: There is no active job for the user
    - Then: An OperationResult is returned with Success=false and ErrorMessage = "There is no active job for the user."
  - When: There are more than one active jobs for the user
    - Then: An OperationResult is returned with Success=false and ErrorMessage = "More than one active job exists for the user, please contact support."

#### 1.4.1.6. \[U\] Job Service - Refresh Process
**Description:**  
When a user has made modifications to the source files and they wish to reload it.
The system shall keep the user's Division and Feed Number selections and reload the files from the beginning.

**Acceptance Criteria:**
- > Given: The user has made modifications to the source files and wishes to reload them and they are still in the original home page (after validation)
  - When: The user presses the "Refresh" button
    - Then: The UI will call RefreshJob(JobId) function which will delete the existing job, create a new one with the division and feed number from the existing job and return it back to the UI.
    - Then: The UI will take the division and feed number from the returning job and populate the UI with it.

- > Given: The user has made modifications to the source files and they wish to reload the files from the beginning.
  - When: The page is refreshed (okta reload), manual user reload, or user returns to the home page.
    - Then: The user will be shown the confirmation dialog because there is an active job. The user will be asked if they wish to resume, cancel or refresh.
      - When: The user clicks "Refresh"
        - Then: The UI will call the RefreshJob(JobId) function which will delete the existing job and create a new one.
        - Then: The UI will take the division and feed number from the returning job and populate the UI with it.


#### 1.4.1.7. \[U\] Job Service - Status management
**Description:**  
As a job progresses from creation, file loading, validation and output file creation, the API shall have the capability of providing updates on the status of a job.

**Acceptance Criteria:**
- > Given: The system is operating on a job
  - When: A particular JobFile has finished an operation
    - Then: The JobService shall update the file status and persist it to the database.
  - When: A particular JobFile has finished Loading
    - Then: The JobService shall update the status of the JobFile to UPLOADED.
  - When: A particular JobFile has finished validating with no errors
    - Then: The JobService shall update the status of the JobFile to VALIDATED.     
  - When: A job file has finished validating with errors  
	  - Then: The status for the JobFile will depend on the validation results of the file.  
    - The expected Job File statuses are: WARNING, ERRORS, CRITICAL.  	
    - The following order shall be followed: 1. CRITICAL, 2. ERRORS, 3. WARNING
  - When: The user accepts the results of the file validation without errors or warnings
    - Then: The status of the JobFile shall be set to "ACCEPTED"    
  - When: The user accepts the results of the file validation with warnings
    - Then: The status of the JobFile shall be set to "ACCEPTED-WARNINGS"
  - When: The user accepts the results of the file validation without errors
    - Then: The status of the JobFile shall be set to "ACCEPTED-ERRORS"
  - When: A Particular JobFile has finished producing an output file.
    - When: The file had no errors
      - Then: The status of the file shall be SUBMITTED
    - When: The file had some errors
      - Then: The status of the file shall be REJECT RECORDS     

#### 1.4.1.8. \[U\] Job Service - Orchestration
**Description:** 
The JobService shall take care of managing all the operations that are required to load, validate and produce output files from start to finish. This will allow the caller or UI to only have to work with a single set of API endpoints which will ensure a simpler interface and reducing the amount of business logic that needs to be present in the UI.

**Acceptance Criteria:**
- > Given: The user wants to select files to validate and the user selects a division, feed number 
  - When: The JobController calls the CreateJob method
    - Then: The JobService will create a new job for the user based on the division and feed number selection.

- > Given: The user wants to validate selected files. The user selects one or more files from the home screen and clicks the "Load Extract Files" button.
  - When: The JobController calls the GetActiveJob method
    - Then: The JobService will look up the existing job, use the FileLoadService to load the files from storage and finally will call the validation service to validate the selected files.

- > Given: The user wants to review the validation results from the validated files. The user clicks selects one or more files from the home screen and presses the "Analyze" button
  - When: The JobController calls the AnalyzeFiles method
    - Then: The JobService will look up the job and return the validation data for the selected files back to the caller.

- > Given: The user has finished working with the files, has reviewed and accepted the validation results. 
  - When: The user has clicked on the "Accept" button and has accepted the last file in the job
    - Then: The JobService will delete any temporary job files and copy the output of the job to the archive directory.

#### 1.4.1.9. \[U\] Job Service - Error Logging
**Description:** 
The JobService shall keep an accurate log of all activities that happen to the job during the lifecycle of the project. As the files are loaded, validated and output files generated, we will add information to the job log. The Job Service shall use the log service to record all the activities.

**Acceptance Criteria:**
- > Given: A new job has been created and files have been added to the job
  - When: The job is saved
    - Then: The listing of files that were added to the job will be added to the job log and persisted to the database.

- > Given: A file has been loaded from the user's storage location
  - When: The file is loaded
    - Then: The listing of files that have been loaded along with number of records and date will be added to the job log.

- > Given: A file has failed to load from the user's storage
  - When: The file cannot be moved
    - Then: The error shall be logged to the job's log.

- > Given: A file is being validated
  - When: The file has failed validation
    - Then: A message stating that the file has failed validation will be added to the job's log.
  - When: The file has been successfully  validated.
    - Then: A message stating that the file has successfully  been validated will be added to the job's log.

- > Given: An output file is being created for a particular input file
  - When: The output file is being created
    - Then: The listing of files being created will be added to the job's log

#### 1.4.1.10. \[U\] Job Service - Create Output File
**Description:** 
After the user has performed the validation and has reviewed the files validation results in the analysis window, the output files are ready to be created. When the user presses the "ACCEPT" button in the analysis window, the UI will instruct the API via the Job Controller to produce the output files.

**Acceptance Criteria**  
- When creating the output files, the service shall create a folder in the user's output folder for storing the output files.
  - If the folder already exists, the files shall be stored inside the existing folder.
  - The name of the folder shall be |month-of-year|YYYY. Example: July2025
  - The month of year shall always be one month less than the curren't month.
- A zip archive shall be created for each validated file with the name of the file being validated without the original extension
  - Example: vla_18_vir_o.txt file being validated will produce vla_18_vir_o.zip
- The contents of the zip file will be as follows:
  - Original file being validated. Example: vla_18_vir_o.txt
    - This file will only contain the records that are **NOT** ERRORS. 
    - Accepted records file: |Original-file-name|.txt
    - Rejected records file: |Original-file-name|_REJECTED.txt
    - Summary file: |Original-file-name|_SUMMARY.txt

#### 1.4.1.11. \[U\] Job Service - Copy Output Files to Supply Chain Folder
**Description:**    
After the output zip files have been created and copied to the user's production folder, the same files must be copied to another folder for further processing by the supply chain team. The files shall be copied to a directory in the application's main-share under the folder SupplyChainCloud/FromDvt_CloudTest (non-prod environment) and SupplyChainCloud/FromDvt_CloudProd (production environment)

**Acceptance Criteria**
- Given: The user has accepted all the files in a validation job
  - When: The output files are being created
    - Then: The system shall copy the corresponding zip files created to the:
      -  main-share/SupplyChainCloud/FromDvt_CloudTest folder for the non-prod environments (dev and stage)
      -  main-share/SupplyChainCloud/FromDvt_CloudProd folder for the production environments
 -  When: The output files are being created and one or more of the files already exist in the target directory:
    -  Then: The system shall overwrite the existing files with the new ones without confirmation.

**Technical Notes:**
- The target path is stored in the config_setting database table with Module: MainShareFolderPaths and the following Names:
  - SupplyChainTargetFolderDevelopment --> Value: SupplyChainCloud/FromDvt_CloudTest
  - SupplyChainTargetFolderStage -- > Value: SupplyChainCloud/FromDvt_CloudTest
  - SupplyChainTargetFolderProduction --> Value: SupplyChainCloud/FromDvt_CloudProd
    

#### 1.4.1.12. \[U\] Job Service - User Log File Generation
**Description:**
After the Job Service with the help of the Validation Service has finished validating the files in the Job, a number of files shall be created in the user's log directory to aid the user in reviewing the issues that were found during the validation process.

**Acceptance Criteria**
- When a file has completed its validation process, three(3) files shall be created in the user's log directory
  - Then: The following files shall be created:
  - Accepted Records File
    - FileName: |original-file-name|_ACCEPTED.txt
    - The file shall only contain rows that are not marked as ERRORS but will include records that are marked as WARNING
    - If there are any records marked CRITICAL, the ACCEPTED file shall not be created.
    - File Header: The file header shall be identical to the original file.
  - Rejected Records File
    - FileName: |original-file-name|_REJECTED.txt
    - The file shall only contain rows that are marked as ERRORS, and CRITICAL
    - File Header: The file header shall contain an additional column on the left called Line Number. This column shall contain the line number of the affected record in the original file. 
    - The file shall be pipe-delimited and contain a final pipe character at the end of each line.
  - Summary File
    - FileName: |original-file-name|_SUMMARY.txt
    - The file shall contain the following:
      - Accepted Records: nnn
      - Rejected Records: nnn
      - Validated by: user-email
      - Validation Timestamp: YYYYMMDD hh:mm
- When: Any of the files already exists in the user's log directory
  - Then: The files shall be overwritten.

### 1.4.2. \[F\] Notification Service (Nice-to-have)

**Description**  
The notification service shall send any required notification emails to users when operations have completed or when any errors occur.

**Benefit Hypothesis**  
By sending notification emails to users upon operation completion or error occurrence, the system will improve user awareness, reduce uncertainty, and enable faster response to issues—leading to increased user satisfaction and operational efficiency.

**Acceptance Criteria**  
- Notification on Success  
  - When an operation completes successfully, a notification email is sent to the relevant user(s).
  - The email includes details such as operation type, completion time, and any relevant output or confirmation.

- Notification on Error
  - When an operation fails or encounters an error, a notification email is sent to the relevant user(s).
  - The email includes error details, timestamp, and suggested next steps or contact information for support.

- Email Delivery Verification
  - Emails are sent using a verified SMTP service or internal mail system.

- User Preferences
  - Users can opt in/out of receiving notifications or customize which types they receive (e.g., only errors, only completions).

- Scalability
  - The notification system can handle concurrent operations and send multiple emails without delay or failure.

- Security
  - Emails do not expose sensitive data and are sent only to authorized recipients.

#### 1.4.2.1. \[U\] Send emails upon completion of a file load/validation activity.
**Description:** 
TBD

**Acceptance Criteria:**
- > Given: TBD

#### 1.4.2.2. \[U\] Send emails upon failure of a file load/validation activity.
**Description:** 
TBD

**Acceptance Criteria:**
- > Given: TBD

#### 1.4.2.3. \[U\] Ability to opt in or out of notifications per user.
**Description:** 
The system shall allow the user to opt in or out of receiving notifications for activity completion and failure.
This setting shall be exposed through an endpoint in the User Preferences Controller.

**Acceptance Criteria:**
- > Given: The user wants to enable/disable notifications for job completions
  - When: The user enables or disables the "Completion Notification" in the settings screen and clicks the "Update" button
    - Then: The system shall persist the setting to the database in the user preferences table.
  - When: The user completes a file validation activity and the "Completion Notification" setting is disabled
    - Then: No email notifications shall not be sent.
  - When: The user completes a file validation activity and the "Completion Notification" setting is enabled
    - Then: The system shall send an email notification to the user with the following information:
      - Division and feed number
      - Load folder location
      - Activity start and end time
      - File listing with same columns as shown in the home screen.

- > Given: The user wants to enable/disable notifications for activity failure.
  - When: The user enables or disables the "Failure Notification" in the settings screen and clicks the "Update" button
    - Then: The system shall persist the setting to the database in the user preferences table.
  - When: The file validation activity fails and the "Failure Notification" setting is disabled
    - Then: No email notifications shall not be sent.
  - When: The file validation activity fails and the "Failure Notification" setting is enabled
    - Then: The system shall send an email notification to the user with the following information:
      - Division and feed number
      - Load folder location
      - Activity start and end time
      - File listing with same columns as shown in the home screen.
      - Summary of failures

### 1.4.3. \[F\] File Load Service (File Load API) 

**Description**  
The file load service allows the DVT to load the text-based source files into an object model that will allow the system to validate the input file.  
The service is made up of several components that follow TDD/BDD principles to allow extensibility to support additional file types.  
The file load service has a reference to an *IFileLoader* which has a LoadFile Method. This method is implemented in all the loaders that are used by the file load service.  
The type of file loader that is instantiated depends on the file type that the file load service is working on.  

The LoadFile method which takes a JobLoadRequest parameter will use the StorageService to fetch the file from storage and get its contents.  
Then the FileLoadService will use a common method called GetRawTextData to take the file contents and convert them to a list of rows of the row type based on the file type that was read. Because all the files are flat-text pipe-delimited files we can have a common method that will parse the file and return a list of strings from the contents of the file.  

As mentioned above, each file type will have its own loader class that implements the IFileLoader. This allows us to put the logic to parse each individual file within its own class which keeps the parsing logic specific to each file separate. If during implementation we find common parsing functions that can be shared amonst the loaders, we can create helper classes are needed.  

Each file type will have a corresponding data row model that will have the fields corresponding to that file type.   
For example: The VIR file will have a *VirDataRow* object that implements *IDataRow*  
*VirDataRow* will have all the fields that each row of the VIR file will have.  
*IDataRow* will have a common property that all rows have which is RowNum.

:::mermaid
classDiagram
        VirFileLoader ..> VirDataRow        
        VirFileLoader ..> IJobFileModel
        VirDataRow ..|> IDataRow
        IJobFileModel ..> JobFileModel
        JobModel ..|> IJobFileModel
        VirFileLoader: +OperationResult LoadFile(FileLoadRequest)
        VirFileLoader: -IJobFileModel ParseVirFileData(FileLoadRequest)
        class IJobFileModel{                        
            +Guid JobFileId
            +Guid JobId            
        }
        class JobFileModel{
            +Guid JobFileId
            +Guid JobId    
            + List~IDataRow~ DataRows
        }     
        class JobModel{
            +Guid JobId    
            +Guid DivisionId
            +int FeedNumber
            +List~IJobFile~ JobFiles
        }   
        class VirDataRow {
          + int RowNum
          + string DivisionId
          + string LocalSite
          + string ReceiptNumber
          + string PoNumber          
        }                
        class IDataRow{
            + int RowNum
        }        
:::

**Benefit Hypothesis**  
 - By implementing the various loaders as individual classes all implementing a common IFileLoader interface, we can easily set up unit test to ensure that our file loaders are correctly converting the flat-file data into object models. This follows TDD best practices.  
-  Separating the logic for the different data files into individual loaders allows for separation of concerns and ensures that we can verify individual components ensuring good code test coverage.

**Acceptance Criteria**  
- Create the FileLoaderService class and create a common method to read the flat-text file into the corresponding object models.
- Create the corresponding file loader classes to parse the contents of the various files into object models.
  - VirFileLoader
  - InventoryFileLoader
  - ItemFileLoader
  - SupplierFileLoader
  - PoItemFileLoader
  - PoFileLoader
  - UomFileLoader
  - MpnFileLoader
- Create the necessary supporting object models to contain the data for each row of the loaded files  
  - VirDataRow
  - InventoryDataRow
  - ItemDataRow
  - SupplierDataRow
  - PoItemDataRow
  - PoDataRow
  - UomDataRow
  - MpnDataRow
- Create necessary unit tests for each of the above-mentioned loaders and service to ensure at least 90% code coverage.  

#### 1.4.3.1. \[U\] File Load Service - Base Components
**Description:** 
Create the basic components to support the File Load Service including:
- JobLoadRequest
- FileLoadRequest
- FileLoadService
- IFileLoadService
- IFileLoader

**Note**: Refer to the lucid diagram (see Useful Links section)

**Acceptance Criteria:**
- JobLoadRequest - Serves as the parameter to the Load Job operation.  
- FileLoadRequest - Serves as the parameter to load an individual file.  
- FileLoadService - Service that loads the raw text file and calls the necessary loader depending on the file type.  
- IFileLoadService - Interface for FileLoadService  
- IFileLoader - Interface for all the individual file loaders.  
- IDataRow - Base class that generically represents a row from any of the types of rows in the application.  
  - The JobFile class has a List of IDataRow. This avoids having to create a JobFile class for each type of file.  

#### 1.4.3.2. \[U\] File Load Service - Vir File Loader
**Description:** 
VIR or Vouchered Invoice Receipts maintains the history of vouchered receipts from Account Payable system. 
The VIR File Loader will take care of reading the text-based content from the VIR file and converting it to a list of VirDataRow representing the rows in the VIR file.

**Field listing**
| Field Name           | Data Type | Length |
|----------------------|-----------|--------|
| DIVISION ID          | VARCHAR2  | 100    |
| LOCAL SITE ID        | VARCHAR2  | 100    |
| RECEIPT NUMBER       | VARCHAR2  | 50     |
| PO NUMBER            | VARCHAR2  | 50     |
| PO LINE NUMBER       | VARCHAR2  | 50     |
| SUPPLIER ID          | VARCHAR2  | 100    |
| PART NUMBER          | VARCHAR2  | 50     |
| SUPPLIER PART NUMBER | VARCHAR2  | 50     |
| QUANTITY ORDERED     | Number    | 15     |
| QUANTITY RECEIVED    | Number    | 15     |
| DATE RECEIVED        | DATE      | 8      |
| INVOICE PRICE PAID   | Number    | 38     |
| UNIT PRICE           | Number    | 38     |
| PURE_LOADED COST     | VARCHAR2  | 50     |
| CURRENCY CODE        | VARCHAR2  | 10     |
| INTRA-DIV            | VARCHAR2  | 10     |
| DIRECT_INDIRECT      | VARCHAR2  | 10     |
| PO TERMS             | VARCHAR2  | 128    |
| FREIGHT TERMS        | VARCHAR2  | 50     |
| UOM                  | VARCHAR2  | 20     |
| TITLE TRANSFER       | VARCHAR2  | 50     |
| PORT                 | VARCHAR2  | 10     |
| RELEASE #            | Number    | 50     |
| COMMITTED DATE       | DATE      | 8      |

**Acceptance Criteria:**
- Create the following components
  - VirDataRow - Contains all the fields defined in the table above
  - VirFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the Vir File
    - ParseVirFileData - Private method to translate the text-based file data into a list of VirDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


#### 1.4.3.3. \[U\] File Load Service - Inventory File Loader
**Description:** 
Inventory table holds a “snapshot” of the available/current inventory on-hand at the time the extract file was generated
The Inventory File Loader will take care of reading the text-based content from the Inventory file and converting it to a list of InventoryDataRow representing the rows in the Inventory file.

**Field listing**
| Field Name     | Data Type | Length |
|----------------|-----------|--------|
| DIVISION ID    | VARCHAR2  | 100    |
| LOCAL SITE ID  | VARCHAR2  | 100    |
| PART NUMBER    | VARCHAR2  | 50     |
| QUANTITY       | Number    | 38     |
| STANDARD COST  | Number    | 38     |
| TOTAL VALUE    | Number    | 38     |
| UOM            | VARCHAR2  | 20     |
| CURRENCY CODE  | VARCHAR2  | 10     |
| PART STATUS    | VARCHAR2  | 50     |
| COMCODE        | VARCHAR2  | 50     |
| DRI CODE       | VARCHAR2  | 50     |
| DESCRIPTION    | VARCHAR2  | 256    |
| INVENTORY DATE | DATE      | 8      |

**Acceptance Criteria:**
- Create the following components
  - InventoryDataRow - Contains all the fields defined in the table above
  - InventoryFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the Inventory File
    - ParseInventoryFileData - Private method to translate the text-based file data into a list of InventoryDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


#### 1.4.3.4. \[U\] File Load Service - Item File Loader
**Description:** 
Specifies the item/part numbers that correspond to a single part ordered from a supplier. Includes all Active direct materials parts. Each part number needs to be mapped to Emerson Corporate commodity/DRI code. 
The Item File Loader will take care of reading the text-based content from the Item file and converting it to a list of ItemDataRow representing the rows in the Item file.

**Field listing**
| Field Name       | Data Type | Length |
|------------------|-----------|--------|
| DIVISION ID      | VARCHAR2  | 100    |
| LOCAL SITE ID    | VARCHAR2  | 100    |
| PART NUMBER      | VARCHAR2  | 50     |
| DESCRIPTION      | VARCHAR2  | 255    |
| COMCODE          | VARCHAR2  | 50     |
| DRI CODE         | VARCHAR2  | 50     |
| PART_STATUS      | VARCHAR2  | 50     |
| DIRECT_INDIRECT  | VARCHAR2  | 50     |
| PURCH_MFRD       | VARCHAR2  | 50     |
| LEAD TIME        | Number    | 50     |
| STANDARD COST    | Number    | 50     |
| PURE_LOADED COST | VARCHAR2  | 50     |
| CURRENCY CODE    | VARCHAR2  | 10     |
| UOM              | VARCHAR2  | 20     |
| ABC CATEGORY     | VARCHAR2  | 10     |
| **ITEM WEIGHT**      | Number    | 50     |
| **ITEM WEIGHT UOM**  | VARCHAR2  | 20     |
| **ITEM HTS CODE**    | VARCHAR2  | 50     |
| **ITEM HS CODE**    | VARCHAR2  | 50     |

**Acceptance Criteria:**
- Create the following components
  - ItemDataRow - Contains all the fields defined in the table above
  - ItemFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the Item File
    - ParseItemFileData - Private method to translate the text-based file data into a list of ItemDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


#### 1.4.3.5. \[U\] File Load Service - Supplier File Loader
**Description:** 
Describes supplier information that is required for FAW reporting. It includes all Active direct material suppliers and valid physical address and telephone number of the supplier. Note: PO Box, care of, & attn to information should NOT be used as the address information. 
The Supplier File Loader will take care of reading the text-based content from the Supplier file and converting it to a list of SupplierDataRow representing the rows in the Supplier file.

**Field listing**
| Field Name      | Data Type | Length |
|-----------------|-----------|--------|
| DIVISION ID     | VARCHAR2  | 100    |
| LOCAL SITE ID   | VARCHAR2  | 100    |
| SUPPLIER ID     | VARCHAR2  | 100    |
| SUPPLIER NAME   | VARCHAR2  | 120    |
| DUNS            | VARCHAR2  | 100    |
| ACTIVE_INACTIVE | VARCHAR2  | 50     |
| DIRECT_INDIRECT | VARCHAR2  | 50     |
| ADDRESS_DESCR   | VARCHAR2  | 50     |
| STREET          | VARCHAR2  | 80     |
| SUITE           | VARCHAR2  | 50     |
| CITY            | VARCHAR2  | 50     |
| STATE           | VARCHAR2  | 50     |
| POSTAL CODE     | VARCHAR2  | 20     |
| COUNTY          | VARCHAR2  | 30     |
| COUNTRY         | VARCHAR2  | 50     |
| ADDR1           | VARCHAR2  | 128    |
| ADDR2           | VARCHAR2  | 128    |
| ADDR3           | VARCHAR2  | 128    |
| ADDR4           | VARCHAR2  | 128    |
| COUNTRY CODE    | VARCHAR2  | 20     |
| GLOBAL FLAG     | VARCHAR2  | 10     |
| MAIN TELEPHONE  | VARCHAR2  | 20     |
| TOLL FREE       | VARCHAR2  | 20     |
| FAX             | VARCHAR2  | 20     |
| WEB SITE        | VARCHAR2  | 50     |
| SUPPLIER TYPE   | VARCHAR2  | 50     |

**Acceptance Criteria:**
- Create the following components
  - SupplierDataRow - Contains all the fields defined in the table above
  - SupplierFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the Supplier File
    - ParseSupplierFileData - Private method to translate the text-based file data into a list of SupplierDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


#### 1.4.3.6. \[U\] File Load Service - PO File Loader
**Description:** 
Purchase Order table holds a “snapshot” of open/closed Purchase Orders available in a month. The intent is to capture unreceipted or partially receipted OPEN purchase orders and CLOSED purchase orders that are associated with the General Ledger Vouchered Receipt period
The PO File Loader will take care of reading the text-based content from the PO file and converting it to a list of PoDataRow representing the rows in the PO file.

**Field listing**
| Field Name       | Data Type |
|------------------|-----------|
| DIVISION ID      | VARCHAR2  |
| LOCAL SITE ID    | VARCHAR2  |
| PO NUMBER        | VARCHAR2  |
| ORDER DATE       | DATE      |
| LATEST AMENDMENT | DATE      |
| COMMODITY MGR ID | VARCHAR2  |
| SUPPLIER ID      | VARCHAR2  |
| CURRENCY CODE    | VARCHAR2  |
| PO TYPE          | VARCHAR2  |
| INTRA-DIV        | VARCHAR2  |
| DIRECT_INDIRECT  | VARCHAR2  |
| PO TERMS         | VARCHAR2  |
| FREIGHT TERMS    | VARCHAR2  |
| EDI              | VARCHAR2  |
| ORDER STATUS     | VARCHAR2  |
| TITLE TRANSFER   | VARCHAR2  |
| PORT             | VARCHAR2  |

**Acceptance Criteria:**
- Create the following components
  - PoDataRow - Contains all the fields defined in the table above
  - PoFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the Po File
    - ParsePoFileData - Private method to translate the text-based file data into a list of PoDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


#### 1.4.3.7. \[U\] File Load Service - PO Item File Loader
**Description:** 
Purchase Order line-item table specifies the detailed information for a single purchase order. Each object in this table holds the details for a single part/item in the purchase order. 
The PoItem File Loader will take care of reading the text-based content from the PoItem file and converting it to a list of PoItemDataRow representing the rows in the PoItem file.

**Field listing**
| Field Name            | Data Type | Length |
|-----------------------|-----------|--------|
| DIVISION ID           | VARCHAR2  | 100    |
| LOCAL SITE ID         | VARCHAR2  | 100    |
| PO NUMBER             | VARCHAR2  | 50     |
| PO LINE NUMBER        | VARCHAR2  | 20     |
| PART NUMBER           | VARCHAR2  | 50     |
| SUPPLIER PART NUMBER  | VARCHAR2  | 50     |
| DESCRIPTION           | VARCHAR2  | 255    |
| CONTRACT ID           |           | 100    |
| UNIT COST             | NUMBER    | 15     |
| PURE_LOADED COST      | VARCHAR2  | 50     |
| ORDERED VALUE         | NUMBER    | 15     |
| QUANTITY ORDERED      | NUMBER    | 15     |
| QUANTITY RETURNED     | NUMBER    | 15     |
| COMMITTED DATE        | DATE      | 8      |
| REQUESTED DATE        | DATE      | 8      |
| ORDER STATUS          | VARCHAR2  | 50     |
| CURRENCY CODE         | VARCHAR2  | 10     |
| UOM                   | VARCHAR2  | 20     |
| QTY LEFT TO RECEIVE   | NUMBER    | 15     |
| VALUE LEFT TO RECEIVE | NUMBER    | 15     |
| RELEASE #             | NUMBER    | 50     |

**Acceptance Criteria:**
- Create the following components
  - PoItemDataRow - Contains all the fields defined in the table above
  - PoItemFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the PoItem File
    - ParsePoItemFileData - Private method to translate the text-based file data into a list of PoItemDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


#### 1.4.3.8. \[U\] File Load Service - UOM File Loader
**Description:** 
The UOM (Unit of Measure) conversion table is used by FAW to maintain business unit/site-specific conversion rates for parts that have different business unit/site UOM’s than the corporate assigned UOM. Each commodity code has a specific corporate identified UOM
The UOM File Loader will take care of reading the text-based content from the UOM file and converting it to a list of UomDataRow representing the rows in the UOM file.

**Field listing**
| Field Name      | Data Type | Length |
|-----------------|-----------|--------|
| Division ID     | VARCHAR2  | 100    |
| Local Site ID   | VARCHAR2  | 100    |
| Part Number     | VARCHAR2  | 50     |
| Local UOM       | VARCHAR2  | 20     |
| Base UOM        | VARCHAR2  | 20     |
| Conversion Rate | NUMBER    | 15     |

**Acceptance Criteria:**
- Create the following components
  - UomDataRow - Contains all the fields defined in the table above
  - UomFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the Uom File
    - ParseUomFileData - Private method to translate the text-based file data into a list of UomDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


#### 1.4.3.9. \[U\] File Load Service - MPN File Loader
**Description:** 
The Manufacturer Part Number (MPN) Information table is used by FAW to maintain the details of each item’s association with an approved Manufacturer and its associated part number(s). This information is used to link a business unit’s part to the preferred material catalog.
The MPN File Loader will take care of reading the text-based content from the MPN file and converting it to a list of MpnDataRow representing the rows in the MPN file.

**Field listing**
| Field Name               | Data Type | Length |
|--------------------------|-----------|--------|
| Division ID              | VARCHAR2  | 256    |
| Local Site ID            | VARCHAR2  | 100    |
| Part Number              | VARCHAR2  | 256    |
| Local Manufacturer ID    | VARCHAR2  | 20     |
| Manufacture ID           | VARCHAR2  | 20     |
| Manufacturer Name        | VARCHAR2  | 128    |
| Manufacturer Part Number | VARCHAR2  | 50     |
| Object ID                | VARCHAR2  | 50     |
| MPN Type                 | VARCHAR2  | 10     |

**Acceptance Criteria:**
- Create the following components
  - MpnDataRow - Contains all the fields defined in the table above
  - MpnFileLoader     
    - Implements the IFileLoader interface
    - LoadFile method - Is called by the FileLoadService to load the Mpn File
    - ParseMpnFileData - Private method to translate the text-based file data into a list of MpnDataRow objects representing the file.
    - Note: Refer to class diagram in feature **File Load Service (File Load API) **


### 1.4.4. \[F\] Master Data Service  

**Description**  
The master data service will act as a repository of data from the APEX system. In the existing solution, this resides as copies of database tables in a local database on the user's computer. Since the primary goal is to replace the application as-is, we will be creating some static tables in DVT's database to house this data. As a further enhancement, we can pursue connecting to the actual APEX system and retrieve live data from it.

The following tables will be included in the Master Data Service:
- Division
- Local Site
- UOM
- Currency
- Commodity Code
- Payment Terms
- Freight Terms (Incoterms)

**Technical Note:** To facilitate being able to replace the static data with APEX later on, we should make the Master Data Service agnostic of the data source. 
We should have methods that will allow us to get a list of the various master data types and methods to check if values are valid without specifically knowing how the underlying data looks like. We should put the logic to interact with the static tables in the repository so that later we can swap the repository with a connection to APEX and the service above will not know any difference.  

**Master Data Table Structure**  
| Column         | Data Type    |
|----------------|--------------|
| ItemId         | UUID         |
| Table          | VARYING(50)  |
| TextId         | VARYING(50)  |
| ItemName       | VARYING(100) |
| ItemNameAbbrev | VARYING(100) |
| Text1          | VARYING(100) |
| Text2          | VARYING(100) |
| Text3          | VARYING(100) |
| Text4          | VARYING(100) |
| Text5          | VARYING(100) |
| Text6          | VARYING(100) |


- Refer to [DVT-MasterData.xlsx](https://emerson.sharepoint.com/:x:/r/sites/GPSTeam/Shared%20Documents/Dev%20Team%20Documents/DVT%20Tool/Design/MasterData/DVT-MasterData.xlsx?d=w46cc4d94e47a445b85f8eaf2fcf1ef38&csf=1&web=1&e=wiqXXm) for the data to populate the table and how the columns are mapped between the source tables and the DVT Master Data table. 
- Refer to [DVT Database Reference](https://emerson.sharepoint.com/:x:/r/sites/GPSTeam/Shared%20Documents/Dev%20Team%20Documents/DVT%20Tool/Design/MasterData/DVT%20Database%20Reference.xlsx?d=wf7b213eee3f84253bc3d71bcef80642c&csf=1&web=1&e=BmArYy) for the original master data.

**Technical Note:** MasterDataValidationResult Structure
```cs
public class MasterDataVerifyResult
{
    public bool Valid
    {
        get
        {
            return Results.Any(r => r.Item2 == false);
        }
    }
  
  public List<Tuple<string, bool>> Results { get; } = new List<Tuple<string, bool>>();
}
```

**Benefit Hypothesis**  
By providing a centralized place for all master data we can ensure that the validation service is able to get the data necessary to validate the files.

**Acceptance Criteria**  
- Provide list of Divisions, Sites, UOM, Currency, Commodity Code, Payment Terms, Freight Terms
- Verify if a given Division is valid
- Verify if a given Local Site is valid
- Verify if a given UOM is valid
- Verify if a given Currency is valid
- Verify if a given Commodity Code is valid
- Verify if a given Payment Term is valid
- Verify if a given Freight Term is valid
- Create necessary unit tests to support the functions added. 


#### 1.4.4.1. \[U\] Master Data Service - List of Master Data 
**Description:** 
- Master data service will be used to get the local version of the APEX master data. Later it can be repurposed to connect with the actual APEX data system.
- Should have some sort of caching to prevent constant data requests.
- Provide the ability to return a list of the various types of master data.
- Put the logic of how to retrieve the data from the single master-data table in the repository level.

**Acceptance Criteria:**
- Return the following lists:
  - Divisions - Sorted by Division Id and Division Name
  - Sites - Sorted by Name
  - UOM - Sorted by Name
  - Currency - Sorted by Name
  - Commodity - Sorted by Name
  - Payment Terms - Sorted by Name 
  - Freight Terms - Sorted by Name 

#### 1.4.4.2. \[U\] Master Data Service - Verify Division
**Description:** 
As the DVT system, I would like to verify if a given list of division Ids is valid so that I can ensure accurate data is used.

**Acceptance Criteria:**
- Create Method in MasterDataRepository and MasterDataService for VerifyDivision
- `Given:` The system is attempting to validate if a given list of Division IDs is valid
  - When: The given Division IDs are checked against the master list of Division IDs
    - Then: The method shall return a MasterDataValidationResult object with the list of Division Id's and a boolean flag signifying if the Division ID is valid.

#### 1.4.4.3. \[U\] Master Data Service - Verify Site
**Description:** 
As the DVT system, I would like to verify if a given list of sites is valid so that I can ensure accurate data is used.

**Acceptance Criteria:**
- Create Method in MasterDataRepository and MasterDataService for VerifySite
- `Given:` The system is attempting to validate if a given list of sites is valid
  - When: The given Sites are checked against the master list of Sites
    - Then: The method shall return a MasterDataValidationResult object with the list of Sites and a boolean flag signifying if the Site is valid.

#### 1.4.4.4. \[U\] Master Data Service - Verify UOM
**Description:** 
As the DVT system, I would like to verify if a given list of UOMs are valid so that I can ensure accurate data is used.

**Acceptance Criteria:**
- Create Method in MasterDataRepository and MasterDataService for VerifyUom
- `Given:` The system is attempting to validate if a given list of UOMs are valid
  - When: The given UOMs are checked against the master list of UOMs
    - Then: The method shall return a MasterDataValidationResult object with the list of UOMs and a boolean flag signifying if the UOM is valid.

#### 1.4.4.5. \[U\] Master Data Service - Verify Currency
**Description:** 
As the DVT system, I would like to verify if a given list of Currencies are valid so that I can ensure accurate data is used.

**Acceptance Criteria:**
- Create Method in MasterDataRepository and MasterDataService for VerifyCurrency
- `Given:` The system is attempting to validate if a given list of currencies are valid
  - When: The given Currencies are checked against the master list of Currencies
    - Then: The method shall return a MasterDataValidationResult object with the list of currencies and a boolean flag signifying if the currency is valid.

#### 1.4.4.6. \[U\] Master Data Service - Verify Commodity
**Description:** 
As the DVT system, I would like to verify if a given list of Commodify UOMs are valid so that I can ensure accurate data is used.

**Acceptance Criteria:**
- Create Method in MasterDataRepository and MasterDataService for VerifyCommodity
- `Given:` The system is attempting to validate if a given list of Commodify UOMs are valid
  - When: The given Commodity UOMs are checked against the master list of Commodity UOMs
    - Then: The method shall return a MasterDataValidationResult object with the list of Commodity UOMs and a boolean flag signifying if the Commodity UOM is valid.

#### 1.4.4.7. \[U\] Master Data Service - Verify Payment Terms
**Description:** 
As the DVT system, I would like to verify if a given list of Payment Terms are valid so that I can ensure accurate data is used.

**Acceptance Criteria:**
- Create Method in MasterDataRepository and MasterDataService for VerifyPaymentTerms
- `Given:` The system is attempting to validate if a given list of Payment Terms are valid
  - When: The given Payment Terms are checked against the master list of Payment Terms
    - Then: The method shall return a MasterDataValidationResult object with the list of Payment Terms and a boolean flag signifying if the Payment Term is valid.

#### 1.4.4.8. \[U\] Master Data Service - Verify Freight Terms
**Description:** 
As the DVT system, I would like to verify if a given list of Freight Terms are valid so that I can ensure accurate data is used.

**Acceptance Criteria:**
- Create Method in MasterDataRepository and MasterDataService for VerifyFreightTerms
- `Given:` The system is attempting to validate if a given list of Freight Terms are valid
  - When: The given Freight Terms are checked against the master list of Freight Terms
    - Then: The method shall return a MasterDataValidationResult object with the list of Freight Terms and a boolean flag signifying if the Freight Term is valid.
  


### 1.4.5. \[F\] Storage Service 

**Description**  
The storage service will allow the DVT system to interact with the Azure File Shares that will serve as the main user and working storage for the application. The user's files will be stored in individual user shares and the application will have an administrative share that will be used to store the working project files. The Storage Service will provide various methods that will allow reading and writing to the file shares throughout the file validation process.

**Benefit Hypothesis**  
To maintain separation of concerns, we will concentrate all operations having to do with file storage within the Storage Service. This will allow us to provide all necessary functions to the application without exposing storage-related code to other areas of the application. In addition, if the file-share storage method does not work and we need to switch to a different type of storage, we only have to make the change in one location in the system.

**Acceptance Criteria**  
- Provide the following functions to the system:
  - Get Folders by email address - Returns the list of folders available in the given user's share.
  - Initialize Job Directory
    - Creates the directory in the working share that will be used by the job.
    - Copies the job files from their source directory to the job directory.
  - Archive Job Files - Creates a zip archive of the job and puts it in the archive directory.
  - Get Files In Directory - Given a directory return the full listing of files in the directory.
  - Delete Job Files - Given a Job Id, it deletes the working folder and all files within it.
  - Get File Contents by Path - Given a path, it will retrieve the contents of a file and return it to the caller.
  - Create a text file given a path and a list of strings.
- Create necessary unit tests to support the functions added. 


#### 1.4.5.1. \[U\] Storage Service - File Retrieval
**Description:** 
Provide the necessary functions required to return files and folders from the application storage to support the following functionalities:  
- Get Folders by Email Address
- Get Files In Directory
- Get File Contents by Path

**Acceptance Criteria:**
- `Get Folders by Email Address`  
  - Given: The system needs to display the available folders within the user's personal share
    - When: The GetFoldersByEmailAddress(string email) method is called 
      - Then: The system shall return the list of folders inside the user's personal share
        - When: The given email address is not the email address of the logged in user.
          - Then: An unauthorized exception shall be raised as users do not have access to other user's personal shares.   
- `Get Files In Directory`  
  - Given: The system needs to find the files matching the file name pattern for a particular type of file in the job.
    - When: The GetFilesInDirectory(string folder) is called
      - Then: The system shall return the full file listing of the directory.        
        - When: The system doe snot find any files
          - Then: An empty list is returned.
- `Get File Contents by Path`  
  - Given: The system needs to get the contents of a file given a full file path
    - When: The GetFileContentsByPath(string path) is called
      - Then: The system shall read the contents of the file and return them as a byte[].
        - When: The system does not find the specified file
          - Then: A not found exception is raised        

#### 1.4.5.2. \[U\] Storage Service - Job Directory Operations
**Description:** 
Provide necessary functions to facilitate the file management for jobs.  
The following functions must be provided:
- Initialize Job Directory
- Delete Job Files
- Archive Job Files

**Acceptance Criteria:**
- `Initialize Job Directory`
  - Given: The system needs to set up the directory structure required to process the files related to the job
    - When: The InitializeJobDirectory(Job) is called
      - Then: A directory is created in the main share under the JobWorkingFolder directory. The name of the directory shall be the JobId of the given job.
      - Then: All the files associated with the job shall be copied from the user's home directory to the job folder created in the previous step.
- `Delete Job Files`
  - Given: The system needs to delete files related to a job due to a job being deleted.
    - When: The DeleteJobFiles(Job) is called
      - Then: The job directory inside the JobWorkingFolder in the main share will be deleted.
- `Archive Job Files`
  - Given: The system needs to archive files after a job has been finished
    - When: The ArchiveJobfiles(Job) is called
      - Then: The service shall create a compressed ZIP archive of the job and save it in the JobArchive folder in the main share.
      - The name of the file shall be id_of_job.zip
- `Unit Tests` - Create necessary unit tests to achieve code coverage.

#### 1.4.5.3. \[U\] Storage Service - Create File
**Description:** 
The Storage Service shall provide the ability for the system to create a text file given a path and a list of strings. This will be used by the OutputFileService to produce the final files after the source files have been validated.

**Acceptance Criteria:**
- `Initialize Job Directory`
  - Given: The OutputFileService needs to produce an output file
    - When: The CreateTextFile(path, name, contents) method is called 
      - Then: The StorageService chall create a text file in the given path with the provided name and content.


### 1.4.6. \[F\] Log Service  

**Description**  
The log service shall log activities for any entity in the system in a common location. 

**Benefit Hypothesis**  
By implementing a service that logs system activities, we will enhance traceability, accountability, and security within the application. 
This will enable faster troubleshooting, support compliance with audit requirements, and provide valuable insights into user behavior and system performance.
By implenting the logging into a single table, we are able to easily find any activity that has been logged with a simple query. 

**Acceptance Criteria**  
- Log Creation - The system shall log all relevant user and system activities, including logins, data changes, errors, and administrative actions.
- Log Format - Each log entry shall include a timestamp, user ID (if applicable), action performed, affected resource, and status (success/failure).
- Real-Time Logging - Activities shall be logged in real-time or near real-time without noticeable delay to the user experience.
- Persistence - Logs shall be stored in a persistent and secure storage system. 
- Create necessary unit tests to support the functions added. 

#### 1.4.6.1. \[U\] Log Service - Create a Log Entry

**Description:** 
The LogService shall provide a facility for other services in the system to record log entries in the system for various activities or errors.

> ActivityLog table structure
- LogId: Unique identifier for each entry.
- EntityId: The identifier of the entity that we are creating a log entry about. Job, JobFile, etc...
- Entity: The name of the entity that the log entry is about. Job, JobFile, etc...
- MessageType: The type of message that we are recording. The INFO, WARNING, ERRORS
- Message: The text of the log entry
- CreateDate: The timestamp of when the entry was created.

**Acceptance Criteria:**
- `Given:` The system needs to log an activity to the database
  - When: The system calls the CreateLog method providing the following required parameters: Entity, EntityId, MessageType and Message
    - Then: The Log Service shall create the log entry with the provided information.
      - When: The system does not provide all the mandatory parameters
        - Then: The Log Service will throw an exception.


### 1.4.7. \[F\] User Service  

**Description**  
The user service is responsible for handling all user-related functions in the system. It should have the capability of retrieving the user from the database and handling the user's load, log, and output path.

Olivia Note: Create feature and stories for this and mark completed then link it to the story that Bob used to create this 11623127

**Benefit Hypothesis**  
By implementing a dedicated user service that handles all user-related functions—including retrieving user data from the database and managing user load and log paths—we will improve modularity, scalability, and maintainability of the system. 

**Acceptance Criteria**  
- `Given:` The system is attempting to log the user into the system
  - When: The UI calls the UserService through the UserController to get the user information by email address
    - Then: The UserService shall look up the user in the database and return the user record back to the caller.
      - When: The user is not found in the database
        - Then: The application shall return a Not Found exception to the caller.
- `Given:` The user wishes to modify the load path or log path
  - When: The user provides a new path for load path and attempts to update.
    - Then: The system shall persist the user's selection to the user's info in the database.
  - When: The user provides a new path for log path and attempts to update.
    - Then: The system shall persist the user's selection to the user's info in the database.
  - When: The user provides a new path for output path and attempts to update.
    - Then: The system shall persist the user's selection to the user's info in the database.
  
#### 1.4.7.1. \[U\] User Service - Get user information
**Description:** 
The application service shall look up the user's information based on email address and user id.

**Acceptance Criteria:**
- > Given: The system is attempting to log the user into the system
  - When: The UI calls the UserService through the UserController to get the user information by email address
    - Then: The UserService shall look up the user in the database and return the user record back to the caller.
      - When: The user is not found in the database
        - Then: The application shall return a Not Found exception to the caller.

#### 1.4.7.2. \[U\] User Service - Persist Log, Load, and Output path selections  
**Description:** 
The User Service shall allow the user to change the preferred path for log file saving and source file load directory.

**Acceptance Criteria:**
- `Given:` The user wishes to modify the load path, log, or output path
  - When: The user provides a new path for load path and attempts to update.
    - Then: The system shall persist the user's selection to the user's info in the database.
  - When: There is an active job for the user
    - Then: The system shall not allow the user to modify the load path until the job is completed or deleted
  - When: The user provides a new path for log path and attempts to update.
    - Then: The system shall persist the user's selection to the user's info in the database.
  - When: The user provides a new path for output path and attempts to update.
    - Then: The system shall persist the user's selection to the user's info in the database.


### 1.4.8. \[F\] Output File Service  

**Description**  
The output file service will create the final version of the file that will be delivered to the user as the result of the validation.
As the user reviews the validation results in the analysis screen, they will accept the results of the validation. The files shall be marked as **ACCEPTED**  

During the validation process, the row number in the files is recorded. This allows us to not have to reprocess the input file when it comes time to produce the output file as we will just skip the rows that are marked as **ERRORS**.

**Benefit Hypothesis**  
The source files and output files both have the same format. This means that we do not have to have individualize the logic to produce the output files. This allows us to have a much simpler service that only focuses 

**Acceptance Criteria**  
- Create the OutputFileService
  - Method for CreateOutputFile(SourceFilePath, TargetFilePath, LinesToSkip)
- Create necessary unit tests to support the functions added. 


#### 1.4.8.1. \[U\] Output File Service - Create Output File
**Description:** 
Using the storage service, read the source file and using the validation information in the JobFile, create the output file skipping any bad rows marked with ERRORS.

**Acceptance Criteria:**
- Method CreateOutputFile
  - Parameters:
    - SourceFilePath - Path to the source file in working job directory
    - TargetFilePath - Path to the output file in target directory
    - LinesToSkip - List of integers of the lines in the source file that need to be skipped.


### 1.4.9. \[F\] Validation Service  

**Description**  
The validation service will take the source data that has been converted from the source text file and apply a series of validation rules.   
The types of rules that will be applied are as follow:
- Static Validation - Checks for string length, date validity, number ranges.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

The validation service will expose a method that will be invoked by the JobService to start the validation process. This method will validate all the files within the job and store the validation results in the validation log for each JobFile. When the validation is completed, the ValidationService will return a JobValidationResult object back to the JobService which will persist the validation information to do the database and return the validation results back to the caller (UI).

The ValidationService, will use the FileType field in the JobFile to determine which IFileValidator will be used to validate the file.  Once that is determined, the ValidationService will create an instance of the appropriate validator and invoke its Validate(IJobFileModel, IJobFileModel1, IJobFileModel2) method passing the IJobFileModel being validated. We must also pass the files that the main file depend on. For example: VIR file depends on the Supplier and Item files.

The IFileValidator will be responsible for all operations related to validating that one type of file. It will use the MasterDataService if needed to get master data during the comparison.

Refer to the [Validation Scenarios](https://emerson.sharepoint.com/:x:/r/sites/GPSTeam/Shared%20Documents/Dev%20Team%20Documents/DVT%20Tool/Design/Validation/Exception%20Scenarios_V4.xlsx?d=wbbf14262db0f4919bff3a22ab311a106&csf=1&web=1&e=Spr0oH) sheet for detailed descriptions of the validation rules.

#### 1.4.9.1. Performance Note on Validation Data Storage ###
The original design was to store the validation results JSON data in the database. However, the size of the files being validated and the verbosity level of the validation results created a performance issue when storing the results in the database. When storing more than 10KB of JSON data, PostgreSQL has to store it on a separate area utilizing TOAST (The Oversized Attribute Storage Technique) which involved compressing and decompressing data on the fly. 

Some of the validation data was observed to be upwards of 50MB which caused the database storage to increase dramatically and the performance to suffer.
As a solution, we're now storing the validation data as JSON files in the working directory for the job. Reading and writing JSON files from the file share is already being done and it's suitable to a file share.


The downside is that we must save the validation results somewhere since the job's working directory is wiped away when the job is completed. The validation data would have remained in the database but now, the JSON files are zipped and saved in the JobArchives folder in the main-share.

**Benefit Hypothesis**  
Separating the validation logic for each file into individual validators will allow us to maintain the logic separated which will improve readability, separation of concerns and allow us to follow TDD principles. Ensuring the validation logic is encapsulated in these individual classes, will allow us to create unit tests and validate the logic in parts.

**Acceptance Criteria**  
- Create the following
  - ValidationService - Handles the overall validation activities
  - Request and result classes - Used to start activities and capture results.
    - FileRowValidationResult
    - FileValidationResult
    - JobValidationResult
  - VirFileValidator
    - VirDataRowStaticValidator
  - InventoryFileValidator
    - InventoryDataRowStaticValidator
  - SupplierFileValidator
    - SupplierDataRowStaticValidator
  - PoItemFileValidator
    - PoItemDataRowStaticValidator
  - PoFileValidator
    - PoFileDataRowStaticValidator
  - UomFileValidator
    - UomDataRowStaticValidator
  - MpnFileValidator
    - MpnDataRowStaticValidator  
- Create necessary unit tests to support the functions added. 

#### 1.4.9.2. \[U\] Validation Service - Validate Vir File
**Description:** 
Perform the necessary operations following the validation rules described in the *Exception Scenarios* document which is linked in the description of the parent feature. 

As was mentioned in the parent feature, to validate the VIR file we must perform a series of validation steps. The VirFileValidator will coordinate all those validation steps and return a comprehensive validation result object back to the ValidationService which will in turn, return it back to the JobService to be shared with the user and saved to the database for further analysis. 

**Validation Rules Summary**  
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**  
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.5. Acceptance Criteria
---  

**Static Validation:**  
- Field: Receipt Number
  - Rules: 
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
  - Message: INVALID FORMAT
  - Status: ERRORS
- Field: Quantity Received
  - Rules: Must be a positive number
  - Message: INVALID VALUE
  - Status: ERRORS
- Field: Invoice Price Paid
  - Rules: Must be a positive number
  - Message: INVALID VALUE
  - Status: ERRORS
- Field: Unit Price
  - Rules: Must be a number
  - Message: INVALID VALUE
  - Status: ERRORS
- Field: Date Received
  - Rules: 
    - Date must be a valid date with the format YYYYMMDD
    - The month component of the date must be less than or equal to the current date's month.
  - Message: INVALID FORMAT
  - Status: ERRORS

**Character Count Validation:**  
The table below shows the character limits for all the fields in the VIR file.  
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name           | Data Type | Length | Mandatory |
|----------------------|-----------|--------|-----------|
| DIVISION ID          | VARCHAR2  | 100    | Y         |
| LOCAL SITE ID        | VARCHAR2  | 100    | Y         |
| RECEIPT NUMBER       | VARCHAR2  | 50     | Y         |
| PO NUMBER            | VARCHAR2  | 50     | Y         |
| PO LINE NUMBER       | VARCHAR2  | 50     | Y         |
| SUPPLIER ID          | VARCHAR2  | 100    | Y         |
| PART NUMBER          | VARCHAR2  | 50     | Y         |
| SUPPLIER PART NUMBER | VARCHAR2  | 50     | N         |
| QUANTITY ORDERED     | Number    | 15     | N         |
| QUANTITY RECEIVED    | Number    | 15     | Y         |
| DATE RECEIVED        | DATE      | 8      | Y         |
| INVOICE PRICE PAID   | Number    | 38     | Y         |
| UNIT PRICE           | Number    | 38     | Y         |
| PURE_LOADED COST     | VARCHAR2  | 50     | Y         |
| CURRENCY CODE        | VARCHAR2  | 10     | Y         |
| INTRA-DIV            | VARCHAR2  | 10     | Y         |
| DIRECT_INDIRECT      | VARCHAR2  | 10     | Y         |
| PO TERMS             | VARCHAR2  | 128    | Y         |
| FREIGHT TERMS        | VARCHAR2  | 50     | N         |
| UOM                  | VARCHAR2  | 20     | Y         |
| TITLE TRANSFER       | VARCHAR2  | 50     | N         |
| PORT                 | VARCHAR2  | 10     | N         |
| RELEASE #            | Number    | 50     | N         |
| COMMITTED DATE       | DATE      | 8      | Y         |

**Mandatory Field Validation**  
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.  
When a mandatory field is missing from a record in the file, the following message and status shall be returned:  
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**  
The first row of the VIR file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.  

When a field is out of order or extra fields are detected, the following message and status shall be returned:  
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL  

**Duplicate Record Validation**  
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:  
|DIVISION ID|+|LOCAL SITE ID|+|RECEIPT_NUMBER|+|PO_NUMBER|+|PO_LINE_NUMBER|+|PART_NUMBER|+|DATE_RECEIVED|+|COMMITTED_DATE|+|RELEASE#|

If a row is found to be a duplicate the following message and status shall be returned:  
Message: DUPLICATE SOURCE RECORD FOUND  
Status: CRITICAL  

**Validate Master Data Dependencies**  
Several fields in the VIR file must exist in the master data tables. 

- Field:  Division Id
  - Rules: Must exist in the Division table.
  - Message: DIVISION ID NOT FOUND
  - Status: CRITICAL
- Field: Local Site Id
  - Rules: Must exist in the Site table.
  - Message: LOCAL SITE ID NOT FOUND
  - Status: CRITICAL
- Field: UOM
  - Rules: Must exist in the list of units of measures.
  - Message: UOM CODE NOT FOUND
  - Status: ERRORS
- Field: Currency Code
  - Rules: Must exist in the list of currency codes.
  - Message: CURRENCY CODE NOT FOUND
  - Status: ERRORS
- Field: Freight Terms
  - Rules: Must exist in the list of Freight Terms.
  - Message: FREIGHT TERM NOT FOUND
  - Status: ERRORS

**Dependent File Field Validation**  
This validation ensures that certain fields in the VIR file are included in dependent files within the same load activity.  
The VIR file depends on the suppliers and item master files. To succesfully load the VIR file, the Supplier and Item master file must also be part of the load package.  

- Field: Part Number
  - Rules: Must exist in the provided parts list in the Item Master file.
    - Part Number and Local Site ID from the VIR file must match PartNumber and Local Site ID in the Item Master File.
  - Message: PART NUMBER NOT FOUND  
  - Status: WARNING
- Field: Supplier Id
  - Rules: Must exist in the provided supplier Ids in the Supplier's file.
    - Supplier Id and Local Site ID from the VIR file must match the PartNumber and Local Site ID in the Supplier File.
  - Message: SUPPLIER ID NOT FOUND  
  - Status: WARNING

**Custom Field Validation**  
Certain fields are validated against a formula or a list of known approved values. The fields are detailed below:  

- Field: Pure Loaded Cost
  - Rules: Value should be P or L
- Field: Direct Indirect
  - Rules: Value must be D
- Field: Intra Div
  - Rules: Value must be N
- Field: Invoice Price Paid
  - Rules: Must be equal to (Quantity Received) x (Unit Price) rounded to 2 decimal places.
    - Example: Quantity = 5, Unit Price = 379.65, Invoice Price Paid shall equal 1898.25
- Message: INVALID VALUE
- Status: ERRORS


#### 1.5.0.1. \[U\] Validation Service - Validate Inventory File
**Description:** 
Perform the necessary operations following the validation rules described in the Exception Scenarios document which is linked in the description of the parent feature.

As was mentioned in the parent feature, to validate the Inventory file we must perform a series of validation steps. The InventoryFileValidator will coordinate all those validation steps and return a comprehensive validation result object back to the ValidationService which will in turn, return it back to the JobService to be shared with the user and saved to the database for further analysis.

**Validation Rules Summary**
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.6. Acceptance Criteria
---
**Static Validation:**  
- Field: Quantity
  - Rules: Must be a positive number greater than 0  
- Field: Total Value
  - Rules: Must be a positive number greater than 0  
- Field: Standard Cost
  - Rules: Must be a positive number greater than 0  
- Field: Inventory Date
  - Rules: 
    - Must be in the format YYYYMMDD
    - Must represent the last day of the previous month. Example: June 2025 submission date shall be 20250531
- Message: INVALID FORMAT
- Status: ERRORS

**Character Count Validation:**
The table below shows the character limits for all the fields in the Inventory file.
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name     | Data Type | Length | Mandatory |
|----------------|-----------|--------|-----------|
| DIVISION ID    | VARCHAR2  | 100    | Y         |
| LOCAL SITE ID  | VARCHAR2  | 100    | Y         |
| PART NUMBER    | VARCHAR2  | 50     | Y         |
| QUANTITY       | Number    | 38     | Y         |
| STANDARD COST  | Number    | 38     | N         |
| TOTAL VALUE    | Number    | 38     | Y         |
| UOM            | VARCHAR2  | 20     | Y         |
| CURRENCY CODE  | VARCHAR2  | 10     | Y         |
| PART STATUS    | VARCHAR2  | 50     | N         |
| COMCODE        | VARCHAR2  | 50     | N         |
| DRI CODE       | VARCHAR2  | 50     | Y         |
| DESCRIPTION    | VARCHAR2  | 256    | N         |
| INVENTORY DATE | DATE      | 8      | Y         |

**Mandatory Field Validation**
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.
When a mandatory field is missing from a record in the file, the following message and status shall be returned:
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**
The first row of the Inventory file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.

When a field is out of order or extra fields are detected, the following message and status shall be returned:
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL

**Duplicate Record Validation**
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:
|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|+|INVENTORY DATE|

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE SOURCE RECORD FOUND
Status: CRITICAL

**Validate Master Data Dependencies**
Several fields in the Inventory file must exist in the master data tables.

- Field: Local Site Id
  - Rules: Must exist in the Organization table.
  - Message: LOCAL SITE ID NOT FOUND
  - Status: CRITICAL
- Field: DRI Code
  - Rules: Must exist in the Commodity Code table.
  - Message: DRI CODE NOT FOUND
  - Status: ERRORS
- Field: UOM
  - Rules: Must exist in the UOM codes table.
  - Message: UOM CODE NOT FOUND
  - Status: ERRORS
- Field: Currency Code
  - Rules: Must exist in the Currency Code table.
  - Message: CURRENCY CODE NOT FOUND
  - Status: ERRORS

**Custom Field Validation**
Certain fields are validated against a formula or a list of known approved values. The fields are detailed below:

- Field: Part Status
  - Rules: Value must only be A, I, O or U
- Message: INVALID VALUE
- Status: ERRORS


#### 1.6.0.1. \[U\] Validation Service - Validate Item File
**Description:** 
Perform the necessary operations following the validation rules described in the Exception Scenarios document which is linked in the description of the parent feature.

**Validation Rules Summary**
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.7. Acceptance Criteria
---
**Static Validation:**  
- Field: Part Number
  - Rules: Can contain any UTF-8 Character
- Field: Part Description
  - Rules: 
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
    - Must not be equal to Part Number
- Field: Standard Cost
  - Rules: Must be a positive integer greater than 0
- Message: INVALID VALUE
- Status: ERRORS

**Character Count Validation:**
The table below shows the character limits for all the fields in the Item file.
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name       | Data Type | Length | Mandatory |
|------------------|-----------|--------|-----------|
| DIVISION ID      | VARCHAR2  | 100    | Y         |
| LOCAL SITE ID    | VARCHAR2  | 100    | Y         |
| PART NUMBER      | VARCHAR2  | 50     | Y         |
| DESCRIPTION      | VARCHAR2  | 255    | Y         |
| COMCODE          | VARCHAR2  | 50     | N         |
| DRI CODE         | VARCHAR2  | 50     | Y         |
| PART_STATUS      | VARCHAR2  | 50     | Y         |
| DIRECT_INDIRECT  | VARCHAR2  | 50     | Y         |
| PURCH_MFRD       | VARCHAR2  | 50     | Y         |
| LEAD TIME        | Number    | 50     | Y         |
| STANDARD COST    | Number    | 50     | Y         |
| PURE_LOADED COST | VARCHAR2  | 50     | Y         |
| CURRENCY CODE    | VARCHAR2  | 10     | Y         |
| UOM              | VARCHAR2  | 20     | Y         |
| ABC CATEGORY     | VARCHAR2  | 10     | Y         |
| ITEM WEIGHT      | Number    | 50     | N         |
| ITEM WEIGHT UOM  | VARCHAR2  | 20     | N         |
| ITEM HTS CODE    | VARCHAR2  | 50     | N         |
| ITEM HS CODE     | VARCHAR2  | 50     | N         |


**Mandatory Field Validation**
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.
When a mandatory field is missing from a record in the file, the following message and status shall be returned:
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**
The first row of the Item file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.

When a field is out of order or extra fields are detected, the following message and status shall be returned:
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL

**Duplicate Record Validation**
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:
|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE SOURCE RECORD FOUND
Status: CRITICAL

**Validate Master Data Dependencies**
Several fields in the Item file must exist in the master data tables.

- Field: Division Id
  - Rules: Must exist in the Organization table.
  - Message: DIVISION ID NOT FOUND
  - Status: CRITICAL
- Field: Local Site Id
  - Rules: Must exist in the Site table.  
    - Local Site Id in the Item file must exist in the site master list
    - The Site Part Reference column shall be used to find a match for the Local Site Id in the Item File
    - Site Part Reference is stored in the Text5 column of the Master Data Table
  - Message: LOCAL SITE ID NOT FOUND
  - Status: CRITICAL
- Field: DRI Code
  - Rules: Must exist in the Commodity Code table.
  - Message: DRI CODE NOT FOUND
  - Status: ERRORS
- Field: UOM
  - Rules: Must exist in the UOM codes table.
  - Message: UOM CODE NOT FOUND
  - Status: ERRORS
- Field: Item Weight UOM
  - Rules: Must exist in the UOM codes table.
  - Message: ITEM WEIGHT UOM NOT FOUND
  - Status: ERRORS
- Field: Currency Code
  - Rules: Must exist in the Currency Code table.
  - Message: CURRENCY CODE NOT FOUND
  - Status: ERRORS


**Custom Field Validation**
Certain fields are validated against a formula or a list of known approved values. The fields are detailed below:

- Field: Part Status
  - Rules: Value must only be A, I, O
- Field: Direct Indirect
  - Rules: Value must only be D
- Field: Purch Mfrd
  - Rules: Value must only be P, M or B
- Field: Pure Loaded Cost
  - Rules: Value must only be P or L
- Field: ABC Category
  - Rules: Values must only be A, AA, B, C, D, D USE, D NEW, D E&O, U
- Message: INVALID VALUE
- Status: ERRORS

#### 1.7.0.1. \[U\] Validation Service - Validate Supplier File
**Description:** 
Perform the necessary operations following the validation rules described in the Exception Scenarios document which is linked in the description of the parent feature.

As was mentioned in the parent feature, to validate the Supplier file we must perform a series of validation steps. The SupplierFileValidator will coordinate all those validation steps and return a comprehensive validation result object back to the ValidationService which will in turn, return it back to the JobService to be shared with the user and saved to the database for further analysis.

**Validation Rules Summary**
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.8. Acceptance Criteria
---
**Static Validation:**  
- Field: Supplier Id
  - Rules: 
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
- Field: Supplier Name
  - Rules: 
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
- Field: Address 1, Address 2, Address 3, Address 4
  - Rules:
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
- Field: Main Telephone
  - Rules: Must be only have numeric characters
- Message: INVALID VALUE
- Status: ERRORS


**Character Count Validation:**
The table below shows the character limits for all the fields in the Supplier file.
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name      | Data Type | Length | Mandatory |
|-----------------|-----------|--------|-----------|
| DIVISION ID     | VARCHAR2  | 100    | Y         |
| LOCAL SITE ID   | VARCHAR2  | 100    | Y         |
| SUPPLIER ID     | VARCHAR2  | 100    | Y         |
| SUPPLIER NAME   | VARCHAR2  | 120    | Y         |
| DUNS            | VARCHAR2  | 100    | N         |
| ACTIVE_INACTIVE | VARCHAR2  | 50     | Y         |
| DIRECT_INDIRECT | VARCHAR2  | 50     | Y         |
| ADDRESS_DESCR   | VARCHAR2  | 50     | N         |
| STREET          | VARCHAR2  | 80     | N         |
| SUITE           | VARCHAR2  | 50     | N         |
| CITY            | VARCHAR2  | 50     | Y         |
| STATE           | VARCHAR2  | 50     | N         |
| POSTAL CODE     | VARCHAR2  | 20     | Y         |
| COUNTY          | VARCHAR2  | 30     | N         |
| COUNTRY         | VARCHAR2  | 50     | Y         |
| ADDR1           | VARCHAR2  | 128    | N         |
| ADDR2           | VARCHAR2  | 128    | N         |
| ADDR3           | VARCHAR2  | 128    | N         |
| COUNTRY CODE    | VARCHAR2  | 20     | Y         |
| GLOBAL FLAG     | VARCHAR2  | 10     | N         |
| MAIN TELEPHONE  | VARCHAR2  | 20     | Y         |
| TOLL FREE       | VARCHAR2  | 20     | N         |
| FAX             | VARCHAR2  | 20     | N         |
| WEB SITE        | VARCHAR2  | 50     | N         |
| SUPPLIER TYPE   | VARCHAR2  | 50     | Y         |


**Mandatory Field Validation**
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.
When a mandatory field is missing from a record in the file, the following message and status shall be returned:
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**
The first row of the Supplier file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.

When a field is out of order or extra fields are detected, the following message and status shall be returned:
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL

**Duplicate Record Validation**
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:
|DIVISION ID|+|LOCAL SITE ID|+|SUPPLIER ID|

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE SOURCE RECORD FOUND
Status: CRITICAL

**Validate Master Data Dependencies**
Several fields in the Supplier file must exist in the master data tables.

- Field: Division Id
  - Rules: Must exist in the Organization table.
  - Message: DIVISION ID NOT FOUND
  - Status: CRITICAL
- Field: Local Site Id
  - Rules: Must exist in the Site table.
    - Local Site Id in the Supplier file must exist in the site master list
    - The Site Supplier Reference column shall be used to find a match for the Local Site Id in the Supplier File
    - Site Supplier Reference is stored in the Text4 column of the Master Data Table
  - Message: LOCAL SITE ID NOT FOUND
  - Status: CRITICAL
- Field: Country Code
  - Rules: Must exist in the Country Code table.
  - Message: COUNTRY CODE NOT FOUND
  - Status: ERRORS
- Field: Country Name
  - Rules: Must exist in the Country table.
  - Message: COUNTRY NAME DOES NOT MATCH
  - Status: ERRORS


**Custom Field Validation**
Certain fields are validated against a formula or a list of known approved values. The fields are detailed below:

- Field: Active Inactive
  - Rules: Value must only be AI, I or U
- Field: Direct Indirect
  - Rules: Value must only be D
- Field: Supplier Type
  - Rules: Value must only be D, M, or B
- Field: Global Flag
  - Rules: Value must only be G, R, U
- Message: INVALID VALUE
- Status: ERRORS

#### 1.8.0.1. \[U\] Validation Service - Validate PO File
**Description:** 
Perform the necessary operations following the validation rules described in the Exception Scenarios document which is linked in the description of the parent feature.

As was mentioned in the parent feature, to validate the PO file we must perform a series of validation steps. The POFileValidator will coordinate all those validation steps and return a comprehensive validation result object back to the ValidationService which will in turn, return it back to the JobService to be shared with the user and saved to the database for further analysis.

**Validation Rules Summary**
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.9. Acceptance Criteria
---
**Static Validation:**  
- Field: PO Number
  - Rules: 
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
  - Message: INVALID FORMAT
  - Status: ERRORS
- Field: Order Date
  - Rules: 
    - Must be in the following format YYYYMMMDD
    - Must be less than 5 years from the current date
    - Must not be a date in the future
  - Message: INVALID FORMAT
  - Status: ERRORS


**Character Count Validation:**
The table below shows the character limits for all the fields in the Inventory file.
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name       | Data Type | Length | Mandatory |
|------------------|-----------|--------|-----------|
| DIVISION ID      | VARCHAR2  | 100    | Y         |
| LOCAL SITE ID    | VARCHAR2  | 100    | Y         |
| PO NUMBER        | VARCHAR2  | 50     | Y         |
| ORDER DATE       | DATE      | 8      | Y         |
| LATEST AMENDMENT | DATE      | 8      | N         |
| COMMODITY MGR ID | VARCHAR2  | 100    | N         |
| SUPPLIER ID      | VARCHAR2  | 100    | Y         |
| CURRENCY CODE    | VARCHAR2  | 10     | Y         |
| PO TYPE          | VARCHAR2  | 20     | Y         |
| INTRA-DIV        | VARCHAR2  | 10     | Y         |
| DIRECT_INDIRECT  | VARCHAR2  | 50     | Y         |
| PO TERMS         | VARCHAR2  | 128    | Y         |
| FREIGHT TERMS    | VARCHAR2  | 50     | N         |
| EDI              | VARCHAR2  | 10     | N         |
| ORDER STATUS     | VARCHAR2  | 50     | Y         |
| TITLE TRANSFER   | VARCHAR2  | 50     | N         |
| PORT             | VARCHAR2  | 10     | N         |


**Mandatory Field Validation**
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.
When a mandatory field is missing from a record in the file, the following message and status shall be returned:
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**
The first row of the Inventory file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.

When a field is out of order or extra fields are detected, the following message and status shall be returned:
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL

**Duplicate Record Validation**
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:
|DIVISION ID|+|LOCAL SITE ID|+|PO NUMBER|

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE SOURCE RECORD FOUND
Status: CRITICAL

**Dependent File Field Validation**  
This validation ensures that certain fields in the PO file are included in dependent files within the same load activity.  
The PO file depends on the suppliers file. To succesfully load the PO file, the Supplier file must also be part of the load package.  

- Field: Supplier Id
  - Rules: Must exist in the provided supplier Ids in the Supplier's file.
    - Supplier Id and Local Site ID from the VIR file must match the PartNumber and Local Site ID in the Supplier File.
  - Message: SUPPLIER ID NOT FOUND  
  - Status: WARNING

**Validate Master Data Dependencies**
Several fields in the PO file must exist in the master data tables.

 Field: Division Id
  - Rules: Must exist in the Organization table.
  - Message: DIVISION ID NOT FOUND
  - Status: CRITICAL
- Field: Local Site Id
  - Rules: Must exist in the Site table.
  - Message: LOCAL SITE ID NOT FOUND
  - Status: CRITICAL
- Field: Freight Term
  - Rules: Must exist in the Freight Terms table.
  - Message: FREIGHT TERM NOT FOUND
  - Status: ERRORS

**Custom Field Validation**
Certain fields are validated against a formula or a list of known approved values. The fields are detailed below:

- Field: Part Status
  - Rules: Value must only be A, I, O or U
- Message: INVALID VALUE
- Status: ERRORS

#### 1.9.0.1. \[U\] Validation Service - Validate PO Item File
**Description:** 
Perform the necessary operations following the validation rules described in the Exception Scenarios document which is linked in the description of the parent feature.

As was mentioned in the parent feature, to validate the PO Item file we must perform a series of validation steps. The PoItemFileValidator will coordinate all those validation steps and return a comprehensive validation result object back to the ValidationService which will in turn, return it back to the JobService to be shared with the user and saved to the database for further analysis.

**Validation Rules Summary**
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.10. Acceptance Criteria
---
**Static Validation:**  
- Field: PO Number, PO Line Number, Part Number
  - Rules:
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
  - Message: INVALID FORMAT
  - Status: ERRORS
- Field: Unit Cost, Quantity Ordered
  - Rules: Must be a numeric value
  - Message: INVALID VALUE
  - Status: ERRORS
- Field: Committed Date
  - Rules: Date must be in valid date format YYYYMMDD
  - Message: INVALID FORMAT
  - Status ERRORS

**Character Count Validation:**
The table below shows the character limits for all the fields in the Inventory file.
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name            | Data Type | Length | Mandatory |
|-----------------------|-----------|--------|-----------|
| DIVISION ID           | VARCHAR2  | 100    | Y         |
| LOCAL SITE ID         | VARCHAR2  | 100    | Y         |
| PO NUMBER             | VARCHAR2  | 50     | Y         |
| PO LINE NUMBER        | VARCHAR2  | 20     | Y         |
| PART NUMBER           | VARCHAR2  | 50     | Y         |
| SUPPLIER PART NUMBER  | VARCHAR2  | 50     | N         |
| DESCRIPTION           | VARCHAR2  | 255    | N         |
| CONTRACT ID           |           | 100    | N         |
| UNIT COST             | NUMBER    | 15     | N         |
| PURE_LOADED COST      | VARCHAR2  | 50     | Y         |
| ORDERED VALUE         | NUMBER    | 15     | Y         |
| QUANTITY ORDERED      | NUMBER    | 15     | Y         |
| QUANTITY RETURNED     | NUMBER    | 15     | N         |
| COMMITTED DATE        | DATE      | 8      | Y         |
| REQUESTED DATE        | DATE      | 8      | N         |
| ORDER STATUS          | VARCHAR2  | 50     | Y         |
| CURRENCY CODE         | VARCHAR2  | 10     | Y         |
| UOM                   | VARCHAR2  | 20     | Y         |
| QTY LEFT TO RECEIVE   | NUMBER    | 15     | Y         |
| VALUE LEFT TO RECEIVE | NUMBER    | 15     | Y         |
| RELEASE #             | NUMBER    | 50     | N         |


**Mandatory Field Validation**
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.
When a mandatory field is missing from a record in the file, the following message and status shall be returned:
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**
The first row of the Inventory file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.

When a field is out of order or extra fields are detected, the following message and status shall be returned:
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL

**Duplicate Record Validation**
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:
|DIVISION ID|+|LOCAL SITE ID|+|PO NUMBER|+|PO LINE NUMBER|+|PART NUMBER|+|COMMITTED DATE|+|REQUESTED DATE|+|RELEASE #|

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE SOURCE RECORD FOUND
Status: CRITICAL

**Dependent File Field Validation**  
This validation ensures that certain fields in the PO Item file are included in dependent files within the same load activity.  
The PO file depends on the PO file. To succesfully load the PO Item file, the PO file must also be part of the load package.  

- Field: PO Number
  - Rules: Must exist in the provided PO Numbers in the associated PO file.
  - Message: PO NUMBER NOT FOUND  
  - Status: WARNING

**Validate Master Data Dependencies**
Several fields in the PO Item file must exist in the master data tables.

Field: Division Id
  - Rules: Must exist in the Organization table.
  - Message: DIVISION ID NOT FOUND
  - Status: CRITICAL
- Field: Local Site Id
  - Rules: Must exist in the Site table.
  - Message: LOCAL SITE ID NOT FOUND
  - Status: CRITICAL
- Field: UOM
  - Rules: Must exist in the UOM codes table.
  - Message: UOM CODE NOT FOUND
  - Status: ERRORS
- Field: Currency Code
  - Rules: Must exist in the Currency Code table.
  - Message: CURRENCY CODE NOT FOUND
  - Status: ERRORS


**Custom Field Validation**
Certain fields are validated against a formula or a list of known approved values. The fields are detailed below:

- Field: Pure Loaded Cost
  - Rules: Value must only be P, L
- Field: Order Status
  - Rules: Value must only be O, C
- Message: INVALID VALUE
- Status: ERRORS

#### 1.10.0.1. \[U\] Validation Service - Validate UOM File
**Description:** 
Perform the necessary operations following the validation rules described in the Exception Scenarios document which is linked in the description of the parent feature.

As was mentioned in the parent feature, to validate the UOM file we must perform a series of validation steps. The UomFileValidator will coordinate all those validation steps and return a comprehensive validation result object back to the ValidationService which will in turn, return it back to the JobService to be shared with the user and saved to the database for further analysis.

**Validation Rules Summary**
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.11. Acceptance Criteria
---
**Static Validation:**  
- Field: Part Number
  - Rules:
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
  - Message: INVALID FORMAT
  - Status: ERRORS
- Field: Conversion Rate
  - Rules:
    - Must be a numeric value with 15 or less figures


**Character Count Validation:**
The table below shows the character limits for all the fields in the Inventory file.
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name      | Data Type | Length | Mandatory |
|-----------------|-----------|--------|-----------|
| Division ID     | VARCHAR2  | 100    | Y         |
| Local Site ID   | VARCHAR2  | 100    | Y         |
| Part Number     | VARCHAR2  | 50     | Y         |
| Local UOM       | VARCHAR2  | 20     | Y         |
| Base UOM        | VARCHAR2  | 20     | Y         |
| Conversion Rate | NUMBER    | 15     | Y         |

**Mandatory Field Validation**
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.
When a mandatory field is missing from a record in the file, the following message and status shall be returned:
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**
The first row of the UOM file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.

When a field is out of order or extra fields are detected, the following message and status shall be returned:
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL

**Duplicate Record Validation**
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:
|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|+|LOCAL UOM|+|BASE UOM|

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE SOURCE RECORD FOUND
Status: CRITICAL

**Dependent File Field Validation**  
This validation ensures that certain fields in the UOM file are included in dependent files within the same load activity.  
The UOM file depends on the Item file. To succesfully load the UOM file, the Item file must also be part of the load package.  

- Field: PART Number
  - Rules: Must exist in the provided Part Numbers in the associated Item file.
  - Message: PART NUMBER NOT FOUND  
  - Status: WARNING

**Validate Master Data Dependencies**
Several fields in the UOM file must exist in the master data tables.

- Field: Division Id
  - Rules: Must exist in the Organization table.
  - Message: DIVISION ID NOT FOUND
  - Status: CRITICAL
- Field: Local Site Id
  - Rules: Must exist in the Site table.
  - Message: LOCAL SITE ID NOT FOUND
  - Status: CRITICAL
- Field: UOM
  - Rules: Must exist in the UOM codes table.
  - Message: UOM CODE NOT FOUND
  - Status: ERRORS


#### 1.11.0.1. \[U\] Validation Service - Validate MPN File
**Description:** 
Perform the necessary operations following the validation rules described in the Exception Scenarios document which is linked in the description of the parent feature.

As was mentioned in the parent feature, to validate the MPN file we must perform a series of validation steps. The MpnFileValidator will coordinate all those validation steps and return a comprehensive validation result object back to the ValidationService which will in turn, return it back to the JobService to be shared with the user and saved to the database for further analysis.

**Validation Rules Summary**
- Static Validation - Checks for static rules in the fields such as number of characters.
- Mandatory Field Validation - Checks for nulls on mandatory fields.
- File Header Validation - Ensure that the file contains the correct columns in the proper order.
- Duplicate Record Validation - Checks for duplicates in the source file using a combination of fields to generate a unique key.
- Validate Master Data Dependencies - Certain fields within files must be present on master data tables such as suppliers, item mater, UOM.
- Dependent Field Validation - Some fields within a file depend on other fields wihtin the same file.
- Custom Field Validation - Some files are validated against a formula. For example Quantity * Unit Price must equal Total Price.

**Technical Note:**
- Static field validation, Mandatory Field Validation, Custom Field validation can be taken care of using Fluent Validation library
- Duplicate Record, Master Data, Dependent Field can be done with custom functions.

1.12. Acceptance Criteria
---
**Static Validation:**  
- Field: Part Number, Manufacturer Part Number
  - Rules:
    - Must contain only UTF-8 characters. 
    - Must not contain the newline or | character
  - Message: INVALID FORMAT
  - Status: ERRORS
- Field: Manufacturer Name
  - Rules: Can contain any UTF-8 Character

**Character Count Validation:**
The table below shows the character limits for all the fields in the Inventory file.
- If a text field exceeds the character limit defined below, the following message and status shall be returned:  
  - Message: CHARACTER LIMIT HAS BEEN EXCEEDED  
  - Status: ERRORS  
- If a numeric field exceeds the character limit defined below, the following message and status shall be returned:
  - Message: IVALID FORMAT
  - Status: ERRORS  

| Field Name               | Data Type | Length | Mandatory |
|--------------------------|-----------|--------|-----------|
| Division ID              | VARCHAR2  | 256    | Y         |
| Local Site ID            | VARCHAR2  | 100    | Y         |
| Part Number              | VARCHAR2  | 256    | Y         |
| Local Manufacturer ID    | VARCHAR2  | 20     | N         |
| Manufacture ID           | VARCHAR2  | 20     | Y         |
| Manufacturer Name        | VARCHAR2  | 128    | Y         |
| Manufacturer Part Number | VARCHAR2  | 50     | Y         |
| Object ID                | VARCHAR2  | 50     | Y         |
| MPN Type                 | VARCHAR2  | 10     | Y         |

**Mandatory Field Validation**
The fields marked as Y in the table above shall be considered mandatory. Fields marked N will be considered as optional.
When a mandatory field is missing from a record in the file, the following message and status shall be returned:
Message: NULL VALUE FOUND IN MANDATORY FIELDS
Status: ERRORS

**File Header Validation**
The first row of the MPN file contains the header for the file. The header columns must be in the proper order to ensure the file is read properly. the column listing in the table above denotes the expected column order.

When a field is out of order or extra fields are detected, the following message and status shall be returned:
Message: HEADER DOES NOT MATCH REQUIRED FORMAT
Status: CRITICAL

**Duplicate Record Validation**
Records in the file must be unique in order for the file to be considered valid. Each record in the file must have a calculated unique key which must be unique within the entire file. The formula for calculating the unique key is as follows:
|DIVISION ID|+|LOCAL SITE ID|+|PART NUMBER|+|MANUFACTURER PART NUMBER|+|LOCAL MANUFACTURER ID|+|MANUFACTURER NAME|

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE SOURCE RECORD FOUND
Status: CRITICAL

**Duplicate Object Id Validation**
The Object ID column is a field that serves as a unique ID for the MPN record. The Object ID field must be unique amongst all the rows in the MPN file.

If a row is found to be a duplicate the following message and status shall be returned:
Message: DUPLICATE OBJECT ID FOUND
Status: ERRORS

**Dependent File Field Validation**  
This validation ensures that certain fields in the MPN file are included in dependent files within the same load activity.  
The UOM file depends on the Item file. To succesfully load the UOM file, the Item file must also be part of the load package.  

- Field: PART Number
  - Rules: Must exist in the provided Part Numbers in the associated Item file.
  - Message: PART NUMBER NOT FOUND  
  - Status: WARNING

**Custom Field Validation**
Certain fields are validated against a formula or a list of known approved values. The fields are detailed below:

- Field: MPN Type
  - Rules: Value must only be P or S
- Message: INVALID VALUE
- Status: ERRORS


####  1.12.0.1. \[U\] Validation Service - Generate statistics Report for Analysis Controller
**Description:** 
The validation process shall produce validation statistics that will be reviewed by the user. Each file type will have different data collected which will be available through the Analysis screen in the application.  

**Technical Note:**  
- Stats can be returned as part of the validation results back to the job service.
- Stats can be saved in a JSON structure in the JobFile table and retrieved when the user wants to review the stats.

**Acceptance Criteria**  

**VIR File**
| Parameter          | MIN                       | MAX                                                       |
| ------------------ | ------------------------- | --------------------------------------------------------- |
| Total Records      | -                         | Total count of records in the file(including bad records) |
| Quantity Ordered   | Lowest Quantity Ordered   | Highest Quantity Ordered                                  |
| Quantity Received  | Lowest Quantity Received  | Highest Quantity Received                                 |
| Date Received      | Oldest Date Received      | Latest Date Received                                      |
| Invoice Price Paid | Lowest Invoice Price Paid | Highest Invoice Price Paid                                |
| Unit Price         | Lowest Unit Price         | Highest Unit Price                                        |
| Committed Date     | Oldest Committed Date     | Latest Committed Date                                     |

**Item Master File**
| Parameter     | MIN                  | MAX                                                       |
| ------------- | -------------------- | --------------------------------------------------------- |
| Total Records | -                    | Total count of records in the file(including bad records) |
| Standard Cost | Lowest Standard Cost | Highest Standard Cost                                     |

**Supplier File**
| Parameter     | MIN | MAX                                                       |
| ------------- | --- | --------------------------------------------------------- |
| Total Records | -   | Total count of records in the file(including bad records) |

**Inventory File**
| Parameter      | MIN                   | MAX                                                       |
| -------------- | --------------------- | --------------------------------------------------------- |
| Total Records  | -                     | Total count of records in the file(including bad records) |
| Quantity       | Lowest Quantity       | Highest Quantity                                          |
| Standard Cost  | Lowest Standard Cost  | Highest Standard Cost                                     |
| Total Value    | Lowest Total Value    | Highest Total Value                                       |
| Inventory Date | Oldest Inventory Date | Latest Inventory Date                                     |

**PO File**
| Parameter        | MIN                   | MAX                                                       |
| ---------------- | --------------------- | --------------------------------------------------------- |
| Total Records    | -                     | Total count of records in the file(including bad records) |
| Order Date       | Oldest Order Date     | Latest Order Date                                         |
| Latest Amendment | Oldest Amendment Date | Latest Amendment Date                                     |

**PO Item File**
| Parameter             | MIN                             | MAX                                                       |
| --------------------- | ------------------------------- | --------------------------------------------------------- |
| Total Records         | -                               | Total count of records in the file(including bad records) |
| Unit Cost             | Lowest Unit Cost                | Highest Unit Cost                                         |
| Ordered Value         | Lowest Ordered Value            | Highest Ordered Value                                     |
| Quantity Ordered      | Lowest Quantity Ordered         | Highest Quantity Ordered                                  |
| Quantity Returned     | Lowest Quantity Returned        | Highest Quantity Returned                                 |
| Committed Date        | Oldest Committed Date           | Latest Committed Date                                     |
| Requested Date        | Oldest Requested Date           | Latest Requested Date                                     |
| Qty Left to Receive   | Lowest Quantity Left to Receive | Highest Quantity Left to Receive                          |
| Value Left to Receive | Lowest Value Left to Receive    | Highest Value Left to Receive                             |

**MPN and Supplier Files**
| Parameter     | MIN | MAX                                                       |
| ------------- | --- | --------------------------------------------------------- |
| Total Records | -   | Total count of records in the file(including bad records) |

**UOM File**
| Parameter       | MIN                    | MAX                                                       |
| --------------- | ---------------------- | --------------------------------------------------------- |
| Total Records   | -                      | Total count of records in the file(including bad records) |
| Conversion Rate | Lowest Conversion Rate | Highest Conversion Rate                                   |


#### 1.12.0.2. \[U\] Validation Service - Validation Message Structure
Validation Storage Structure

```json
"FileValidationResult":{
  "FileName":"abc_123.txt",
  "ValidationRows":[
    {
      "Row":1,
      "Status":"ERRORS",
      "Columns":[
        {
          "Name":"PO Number",
          "Message":"PO Number is missing"
        },
        {
          "Name":"Item Number",
          "Message":"Item Number must be between 1 and 10 characters. You entered 20 characters." 
        }
      ]
    }
  ]
}
```

## 1.13. Priority List
1. Storage Service
2. User Service
3. Master Data Service
4. File Output Service
5. About Controller
6. UserInfo Controller
7. Storage Controller  
8. Master Data Controller
9. File Load Service (Vir)
10. File Validation Service (Vir)
11. Job Controller
12. Analysis Controller
13. Option List Controller


## 1.14. Useful Links
- [DVT Lucid Diagrams](https://lucid.app/lucidchart/4e1ad5bd-c2c7-452d-8075-8a6caff0b6db/edit?viewport_loc=-6716%2C579%2C2977%2C1407%2Ca3bgFFB2b~du&invitationId=inv_c452872f-45e5-482c-a587-1fd6d6a4761e)
- [Mermaid Diagram Extension](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid)
- [DVT Database Reference](https://emerson.sharepoint.com/:x:/r/sites/GPSTeam/Shared%20Documents/Dev%20Team%20Documents/DVT%20Tool/Design/MasterData/DVT%20Database%20Reference.xlsx?d=wf7b213eee3f84253bc3d71bcef80642c&csf=1&web=1&e=KgcitR)
- [DVT Master Data Static Definition](https://emerson.sharepoint.com/:x:/r/sites/GPSTeam/Shared%20Documents/Dev%20Team%20Documents/DVT%20Tool/Design/MasterData/DVT-MasterData.xlsx?d=w46cc4d94e47a445b85f8eaf2fcf1ef38&csf=1&web=1&e=PfCBTM)
