% GrowthRate.m
% Determine the growth rate given a starting value and multiplier.

% Clear data from previous runs.
clc;clear;clf;close all;

start = 20; % Starting value (i.e. 20% density).
mult = [1.2 1.3 1.35 1.4];  % Vector of multipliers.

% figure
numSubplots = size(mult,2);

numValues = 10; % No. of times to apply the multiplier.
values = zeros(numSubplots, numValues); % Resulting values after each mult.

for m=1:numSubplots
    values(m,1) = start;
    for i=2:numValues
        values(m,i) = values(m,i-1) * mult(m);
    end
    
%     subplot(numSubplots,1,m);
%     plot(values(m,1:end),'o');    
end

% Display results to command window.
values
