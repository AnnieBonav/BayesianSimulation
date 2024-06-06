# Bayesian Inference in Agent Simulation

## Overview
This document explains the Bayesian inference mechanism implemented in the agent simulation code. The goal is to use Bayesian methods to choose the most appropriate activity for an agent based on its current states (needs).

## Naive Bayes Classifier

The Naive Bayes classifier is used to predict the probability of different activities given the current states of the agent. The classifier assumes that each state is conditionally independent given the activity.

### Basic Formula

The Naive Bayes formula can be written as:

\[ P(A|S_1, S_2, ..., S_n) = \frac{P(A) \cdot P(S_1|A) \cdot P(S_2|A) \cdot ... \cdot P(S_n|A)}{P(S_1, S_2, ..., S_n)} \]

Where:
- \( P(A|S_1, S_2, ..., S_n) \) is the posterior probability of activity \( A \) given states \( S_1, S_2, ..., S_n \).
- \( P(A) \) is the prior probability of activity \( A \).
- \( P(S_i|A) \) is the likelihood of state \( S_i \) given activity \( A \).
- \( P(S_1, S_2, ..., S_n) \) is the evidence, a normalizing constant ensuring the probabilities sum to 1.


### Concepts
#### Variance

Variance is a statistical measure that describes the spread of a set of data points. It quantifies how much the values in a dataset deviate from the mean (average) of the dataset. A higher variance indicates that the data points are more spread out from the mean, while a lower variance indicates that they are closer to the mean.

##### Formula

The variance of a set of values \( \{x_1, x_2, ..., x_n\} \) with mean \( \mu \) is calculated using the formula:

\[ \text{Variance} = \frac{1}{n} \sum_{i=1}^{n} (x_i - \mu)^2 \]

Where:
- \( n \) is the number of values in the dataset.
- \( x_i \) is each individual value in the dataset.
- \( \mu \) is the mean of the dataset.

##### Code Implementation

In the provided code, the variance is calculated as follows:

```csharp
protected float Variance(List<float> values, float mean)
{
    return values.Select(v => (v - mean) * (v - mean)).Sum() / values.Count;
}
```

### Gaussian Probability Calculation

This method calculates the Gaussian (or Normal) probability density function (PDF) for a given \( x \), mean, and variance. The Gaussian PDF is widely used in statistics, natural sciences, and social sciences as a simple model for random variables whose distributions are not known. It forms a bell-shaped curve, determined by two parameters: the mean (the peak of the bell curve) and the variance (how wide or narrow the bell curve is).

#### Formula

The Gaussian probability density function is given by:

\[ P(x|\mu, \sigma^2) = \frac{1}{\sqrt{2\pi\sigma^2}} \exp\left(-\frac{(x - \mu)^2}{2\sigma^2}\right) \]

Where:
- \( x \) is the value for which the probability is being calculated.
- \( \mu \) is the mean (average value).
- \( \sigma^2 \) is the variance (a measure of how spread out the values are).

#### Code Implementation

In the provided code, the Gaussian probability is calculated as follows:

```csharp
protected float GaussianProbability(float x, float mean, float variance)
{
    return (1 / Mathf.Sqrt(2 * Mathf.PI * variance)) * Mathf.Exp(-((x - mean) * (x - mean)) / (2 * variance));
}
```

### Likelihoods
## Calculate Likelihoods Method

### Concept

This method, `CalculateLikelihoods`, is used to calculate the prior probabilities and likelihoods (mean and variance) for each activity type in an agent's activities. This is crucial for performing Bayesian inference, which uses these probabilities to infer the probability of each activity type given the current state.

### Code Implementation

In the provided code, the likelihoods and priors are calculated as follows:

```csharp
protected void CalculateLikelihoods()
{
    // Calculate priors and likelihoods (mean and variance)
    foreach (ACTIVITY_TYPE activityType in agent.Activities)
    {
        float prior = (float)activityCounts[activityType] / totalData;
        
        List<GaussianInfo> statesData = new List<GaussianInfo>();

        foreach (STATE_TYPE stateType in agentsPerformedActivities.Keys)
        {
            float stateMean = CalculateAverage(agentsPerformedActivities[stateType][activityType]);
            float stateVariance = Variance(agentsPerformedActivities[stateType][activityType], stateMean);
            // Create a GaussianInfo object with the calculated mean and variance
            GaussianInfo stateData = new GaussianInfo(stateType, stateMean, 0, 100, stateVariance);
            statesData.Add(stateData);
        }

        PerformedActivityData activityData = new PerformedActivityData(prior, statesData, activityType);
        
        performedActivitiesData[activityType] = activityData;
    }
}
```



### Implementation Steps

1. **Compute Priors:**
   The prior probability of each activity is stored and represents how likely each activity is before observing any states.

   ```csharp
   float prior = performedActivityData.Prior;

2. **Compute Likelihoods:**
   For each state, compute the likelihood \( P(S_i|A) \) using a Gaussian (Normal) distribution, assuming state values are normally distributed given the activity.
   
   ```csharp
   float stateLikelihood = GaussianProbability(stateData.Value, performedActivityData.StatesData[stateData.StateType].Mean, performedActivityData.StatesData[stateData.StateType].Variance);

   The Gaussian probability density function is given by:

\[ P(x|\mu, \sigma^2) = \frac{1}{\sqrt{2\pi\sigma^2}} \exp\left(-\frac{(x - \mu)^2}{2\sigma^2}\right) \]

Where:

- \( x \) is the observed state value.
- \( \mu \) is the mean state value for the activity.
- \( \sigma^2 \) is the variance of the state value for the activity.

3. **Compute Posteriors:**
   Multiply the prior by the likelihoods of all states to get the unnormalized posterior probability for each activity.
    ```csharp
    float posterior = prior;
    foreach (StateData stateData in currentStateValues.StatesValues)
    {
        float stateLikelihood = GaussianProbability(stateData.Value, performedActivityData.StatesData[stateData.StateType].Mean, performedActivityData.StatesData[stateData.StateType].Variance);
        posterior *= stateLikelihood;
    }

3. **Normalize Posteriors:**
    Normalize the posterior probabilities so they sum to 1. This step is implicit in the code when comparing posterior probabilities directly.
    ```csharp
    Dictionary<ACTIVITY_TYPE, float> posteriorProbabilities = new Dictionary<ACTIVITY_TYPE, float>();
    // After computing posteriors for all activities
    ACTIVITY_TYPE chosenActivity = posteriorProbabilities.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

#### Example calculation
# Bayesian Inference in Agent Simulation

## Overview
This document explains the Bayesian inference mechanism implemented in the agent simulation code. The goal is to use Bayesian methods to choose the most appropriate activity for an agent based on its current states (needs).

## Naive Bayes Classifier

The Naive Bayes classifier is used to predict the probability of different activities given the current states of the agent. The classifier assumes that each state is conditionally independent given the activity.

### Basic Formula

The Naive Bayes formula can be written as:

\[ P(A|S_1, S_2, ..., S_n) = \frac{P(A) \cdot P(S_1|A) \cdot P(S_2|A) \cdot ... \cdot P(S_n|A)}{P(S_1, S_2, ..., S_n)} \]

Where:
- \( P(A|S_1, S_2, ..., S_n) \) is the posterior probability of activity \( A \) given states \( S_1, S_2, ..., S_n \).
- \( P(A) \) is the prior probability of activity \( A \).
- \( P(S_i|A) \) is the likelihood of state \( S_i \) given activity \( A \).
- \( P(S_1, S_2, ..., S_n) \) is the evidence, a normalizing constant ensuring the probabilities sum to 1.

### Implementation Steps

Suppose we have the following states and activities:

- **States**: Bathroom need (50), Sleep need (30), Food need (60)
- **Activities**: Bathroom, Sleep, Eat, Relax

### Step-by-Step Calculation

1. **Compute Priors**:
   Let's assume equal priors for simplicity:
   \[ P(\text{Bathroom}) = P(\text{Sleep}) = P(\text{Eat}) = P(\text{Relax}) = 0.25 \]

2. **Compute Likelihoods**:
   For each state given an activity, assume we have the following means and variances (hypothetical values):

| Activity  | State           | Mean | Variance |
|-----------|-----------------|------|----------|
| Bathroom  | Bathroom need   | 80   | 10       |
| Bathroom  | Sleep need      | 20   | 5        |
| Bathroom  | Food need       | 20   | 5        |
| Sleep     | Bathroom need   | 20   | 5        |
| Sleep     | Sleep need      | 70   | 10       |
| Sleep     | Food need       | 30   | 5        |
| Eat       | Bathroom need   | 30   | 5        |
| Eat       | Sleep need      | 20   | 5        |
| Eat       | Food need       | 70   | 10       |
| Relax     | Bathroom need   | 40   | 5        |
| Relax     | Sleep need      | 40   | 5        |
| Relax     | Food need       | 40   | 5        |

Compute the likelihood for each state given an activity using the Gaussian formula. For instance, for Bathroom need = 50 and Activity = Bathroom:

\[ P(\text{Bathroom need} = 50 \mid \text{Bathroom}) = \frac{1}{\sqrt{2 \pi \cdot 10}} \exp \left( -\frac{(50 - 80)^2}{2 \cdot 10} \right) \]

3. **Compute Posteriors**:
   Multiply the prior by the likelihoods of all states for each activity.

   For Bathroom:

   \[ P(\text{Bathroom} \mid \text{Bathroom need} = 50, \text{Sleep need} = 30, \text{Food need} = 60) \propto P(\text{Bathroom}) \cdot P(\text{Bathroom need} = 50 \mid \text{Bathroom}) \cdot P(\text{Sleep need} = 30 \mid \text{Bathroom}) \cdot P(\text{Food need} = 60 \mid \text{Bathroom}) \]

4. **Normalize and Choose Activity**:
   Normalize the posterior probabilities and choose the activity with the highest posterior probability.

   \[ P(\text{Bathroom} \mid \text{States}) = \frac{P(\text{Bathroom} \mid \text{States})}{P(\text{Bathroom} \mid \text{States}) + P(\text{Sleep} \mid \text{States}) + P(\text{Eat} \mid \text{States}) + P(\text{Relax} \mid \text{States})} \]

   The activity with the highest normalized posterior probability is chosen.

