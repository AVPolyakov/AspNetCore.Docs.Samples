# Подмена сервисов через статический метод

В тестах сервис можно заменить с помощью статического метода `SetCurrent`:
```csharp
ISampleService service = ...
Service.SetCurrent(service)
```

Если current равно `null`, то используется сервис, ранее зарегистрированный в DI контейнере.

Подключение в DI контейнер происходит с помощью метода `DecorateByTestServices`.
