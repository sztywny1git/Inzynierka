using UnityEngine;

public class AbilityRunner : PoolableObject
{
    private IStepAbility _steppableAbility;
    private AbilityContext _context;
    private AbilitySnapshot _snapshot;
    private object _state;
    
    private int _currentIndex;
    private int _totalCount;
    private float _timer;
    private float _delay;

    public void Initialize(
        IStepAbility ability,
        AbilityContext context, 
        AbilitySnapshot snapshot, 
        int count, 
        float delay,
        object state = null)
    {
        _steppableAbility = ability;
        _context = context;
        _snapshot = snapshot;
        _state = state;
        
        _currentIndex = 0;
        _totalCount = count;
        _delay = delay;
        _timer = 0f;
    }

    public T GetState<T>() where T : class
    {
        return _state as T;
    }

    protected override void Update()
    {
        base.Update();

        if (_context.Origin == null)
        {
            ReturnToPool();
            return;
        }
        
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
            return;
        }

        _steppableAbility.OnRunnerStep(_context, _snapshot, _currentIndex, this);

        _currentIndex++;
        
        if (_currentIndex >= _totalCount)
        {
            ReturnToPool();
        }
        else
        {
            _timer = _delay;
        }
    }
}