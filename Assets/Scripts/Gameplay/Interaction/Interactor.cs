using UnityEngine;
using System.Collections.Generic;
using VContainer;

public class Interactor : MonoBehaviour
{
    private readonly List<IInteractable> _interactablesInRange = new List<IInteractable>();
    private IInteractable _closestInteractable;
    
    private UIEventBus _uiEventBus;
    private IObjectResolver _container;

    [Inject]
    public void Construct(UIEventBus uiEventBus, IObjectResolver container)
    {
        _uiEventBus = uiEventBus;
        _container = container;
    }
    
    public void PerformInteraction()
    {
        if (_closestInteractable != null)
        {
            var context = new InteractionContext(
                this.gameObject,
                (_closestInteractable as MonoBehaviour)?.gameObject,
                _container);
            
            _closestInteractable.Interact(context);
        }
    }

    private void Update()
    {
        FindClosestInteractable();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (!_interactablesInRange.Contains(interactable))
            {
                _interactablesInRange.Add(interactable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {    
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            _interactablesInRange.Remove(interactable);
        }
    }

    private void FindClosestInteractable()
    {
        IInteractable closest = null;
        float minDistanceSqr = float.MaxValue;

        for (int i = _interactablesInRange.Count - 1; i >= 0; i--)
        {
            var interactable = _interactablesInRange[i];
            var monoBehaviour = interactable as MonoBehaviour;
            if (monoBehaviour == null || !monoBehaviour.isActiveAndEnabled)
            {
                _interactablesInRange.RemoveAt(i);
                continue;
            }
            float distanceSqr = (transform.position - monoBehaviour.transform.position).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                closest = interactable;
            }
        }
        
        if (_closestInteractable != closest)
        {
            _closestInteractable = closest;
            _uiEventBus.UpdateInteractionTooltip(_closestInteractable?.GetInteractText());
        }
    }
}