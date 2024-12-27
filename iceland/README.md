# Codegen for Mercury

Generates c# code for uisng objects in database including Tables and StoreProcedures. Initially this tool only supports Mercury code.

## Features

- Generate code for Entities using EF Core scaffolding cli
- Generate StoreProcedures
  - Input Parameters
  - Output Parameters
  - Single Result i.e SP with Single Select statement as result.
  - Multi Result set , i.e. SPs with Multiple results
- Output Code
  - Entities
  - StoreProcedures
  - Context

## TODO Next

- Support for generating Mercury-based code i.e. target=Mercury
- Support Async version of Execute() method for StoreProcedures

## Usage Examples

### Basic:

![alt text](images/basic.png)

### MultiResultSet:

![alt text](images/multi.png)

### List of Procedures:

![alt text](images/list.png)

# How to Generate Code

cd to -> src/Codegen and execute "./run.ps1" file
