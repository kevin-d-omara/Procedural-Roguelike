function randomValue = RandomBetween(min, max)
% Get a random value between min and max exclusive.
    randomValue = (max-min)*rand + min;
end