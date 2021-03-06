﻿using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace SGI.Core
{
    public class TaskAwareObservable<T> : IObservable<T>, IDisposable
    {
        private readonly Task _task;
        private readonly Subject<T> _subject;
        private readonly CancellationTokenSource _taskCancellationTokenSource;

        public TaskAwareObservable(Subject<T> subject, Task task, CancellationTokenSource tokenSource)
        {
            _task = task;
            _subject = subject;
            _taskCancellationTokenSource = tokenSource;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var disposable = _subject.Subscribe(observer);
            if (_task.Status == TaskStatus.Created)
                _task.Start();
            return disposable;
        }

        public void Dispose()
        {
            // cancel consumption and wait task to finish
            _taskCancellationTokenSource.Cancel();
            _task.Wait();

            // dispose tokenSource and task
            _taskCancellationTokenSource.Dispose();
            _task.Dispose();

            // dispose subject
            _subject.Dispose();
        }
    }
}